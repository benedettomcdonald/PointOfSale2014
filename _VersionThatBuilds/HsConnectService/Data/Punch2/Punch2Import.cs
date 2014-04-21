using System;
using System.IO;
using System.Collections;
using System.Globalization;

namespace HsConnect.Data.Punch2
{
	public class Punch2Import
	{
		public Punch2Import( String fileName )
		{
			this.fileName = fileName;
		}

		private static Hashtable adjSerials = new Hashtable();
		private static Hashtable empCards = new Hashtable();
		private static Hashtable tcBySerial = new Hashtable();
		private String fileName = "";
		private ArrayList itemList = new ArrayList();

		public ArrayList GetPunch2TimeCards()
		{
			this.itemList.Clear();
			try
			{
				StreamReader reader = File.OpenText( fileName );
				while( reader.Peek() > 0 )
				{
					ProcessLine( reader.ReadLine() );
				}
				foreach( Punch2Item item in itemList )
				{
					if( item.PunchType == Punch2Item.PUNCH_IN )
					{
						if( empCards.ContainsKey( item.EmpNumber.ToString() ) ) empCards.Remove( item.EmpNumber.ToString() );
						Punch2TimeCard tc = new Punch2TimeCard( item );
						empCards.Add( item.EmpNumber.ToString(), tc );
						tcBySerial.Add( item.SerialNumber.ToString(), tc );
					}
					if( item.PunchType == Punch2Item.PUNCH_OUT )
					{
						if( empCards.ContainsKey( item.EmpNumber.ToString() ) )
						{
							Punch2TimeCard tc = (Punch2TimeCard) empCards[ item.EmpNumber.ToString() ];
							Punch2TimeCard newTc = (Punch2TimeCard) tcBySerial[ tc.SerialNumberIn.ToString() ];
							newTc.OutTime = item.OutTime;
						}
					}
					if( item.PunchType == Punch2Item.ADJUSTED )
					{
						Punch2Adjust adj = (Punch2Adjust) item;
						if( GetSerial( adj.SerialNumber ) < 0 )
						{
							Punch2TimeCard tc = new Punch2TimeCard( adj );
							tcBySerial.Add( adj.SerialNumber.ToString(), tc );
						}
						else
						{
							Punch2TimeCard newTc = (Punch2TimeCard) tcBySerial[ GetSerial( adj.SerialNumber ).ToString() ];
							newTc.InTime = adj.InTime;
							newTc.OutTime = adj.OutTime;
							newTc.Date = adj.Date;
							newTc.JobNumber = adj.JobNumber;
						}
					}
				}
				ArrayList tcList = new ArrayList( tcBySerial.Values );
				return tcList;
			}
			catch( Exception ex )
			{
				Console.WriteLine( ex.ToString() );
			}
			return new ArrayList();
		}

		public void ProcessLine( String line )
		{
			string[] tkns = line.Split( new char[] {','} );
			int punchType = Convert.ToInt32( tkns[3] );
			switch( punchType )
			{
				case 1:
					Punch2Item inItem = new Punch2Item( Convert.ToInt32( tkns[0] ), Convert.ToInt32( tkns[1] ), Convert.ToInt32( tkns[2] ), Convert.ToInt32( tkns[3] )  );
					inItem.Date = GetDate( tkns[5] );
					inItem.InTime = GetTime( tkns[6] );
					this.itemList.Add( inItem );
					break;
				case 2:
					Punch2Item outItem = new Punch2Item( Convert.ToInt32( tkns[0] ), Convert.ToInt32( tkns[1] ), Convert.ToInt32( tkns[2] ), Convert.ToInt32( tkns[3] )  );
					outItem.Date = GetDate( tkns[5] );
					outItem.OutTime = GetTime( tkns[6] );
					this.itemList.Add( outItem );
					break;
				case 4:
                    Punch2Adjust adjItem = new Punch2Adjust( Convert.ToInt32( tkns[0] ), Convert.ToInt32( tkns[1] ), Convert.ToInt32( tkns[2] ), Convert.ToInt32( tkns[3] )  );
					if( !tkns[4].Equals("00/00/00") ) adjItem.Date = GetDate( tkns[4] );
					adjItem.InTime = GetTime( tkns[5] );
					adjItem.OutTime = GetTime( tkns[6] );
					adjItem.AdjSerialNumber = Convert.ToInt32( tkns[7] );
					UpdateHash( adjItem.AdjSerialNumber, Convert.ToInt32( tkns[0] ) );
					if( !tkns[8].Equals("00/00/00") ) adjItem.AdjDate = GetDate( tkns[8] );
					adjItem.AdjInTime = GetTime( tkns[9] );
					adjItem.AdjOutTime = GetTime( tkns[10] );
					adjItem.AdjJobNumber = Convert.ToInt32( tkns[11] );
					this.itemList.Add( adjItem );
                    break;
			}
		}

		private void UpdateHash( int oldSerial, int newSerial )
		{
			int origSerial = newSerial;
			if( oldSerial != -1 )
			{
				origSerial = GetSerial( oldSerial ) == -1 ? oldSerial : GetSerial( oldSerial );
				AddSerial( newSerial, origSerial );
			}
		}

		private TimeSpan GetTime( String str )
		{
			String[] tkns = str.Split( new char[] {':'} );
			int hrs = Convert.ToInt32( tkns[0] );
			int mins = Convert.ToInt32( tkns[1] );
			if( hrs > 23 ) return new TimeSpan(0,0,0,1);
			return new TimeSpan( hrs, mins, 0 );
		}

		private DateTime GetDate( String str )
		{
			DateTimeFormatInfo myDTFI = new CultureInfo( "en-US", false ).DateTimeFormat;
			return DateTime.ParseExact( str, "yy/MM/dd", myDTFI );
		}

		public static void AddSerial( int adjNumber, int serial )
		{
			adjSerials.Add( adjNumber.ToString(), serial.ToString() );
		}

		public static int GetSerial( int adjNumber )
		{
			if( adjSerials.ContainsKey( adjNumber.ToString() ) )
			{
				return Convert.ToInt32( (String) adjSerials[ adjNumber.ToString() ] );
			}else return -1;
		}

	}
}
