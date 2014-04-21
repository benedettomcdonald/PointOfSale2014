using HsConnect.Data;
using HsSharedObjects.Client.Preferences;
using HsConnect.SalesItems;
using HsConnect.Forms;

using HsSharedObjects;
using System;
using System.Data;
using System.Collections;
using System.Threading;
using System.Text;
using System.IO;
using Microsoft.Data.Odbc;

namespace HsConnect.SalesItems.PosList
{
	public class AppleOneSalesItemList : SalesItemListImpl
	{
		public AppleOneSalesItemList(){}

		public override float GetSalesTotal()
		{
			float ttl = 0.0f;
			return ttl;
		}

		private HsData data = new HsData();
        public override void DbLoad()
        {
            StreamReader text = null;
            DateTime nextDate = DateTime.Today;
            logger.Debug("before forLoop:  " + nextDate.ToString("yyyyMMdd"));
            bool addComps = false;
            bool addVoids = false;
            bool addDiscs = false;
            if (this.Details.Preferences.PrefExists(1021))
            {
                logger.Debug("Using pref 1021");
                Preference pref = this.Details.Preferences.GetPreferenceById(1021);
                if (pref != null)
                {
                    addComps = pref.Val2.Equals("1");
                    addVoids = pref.Val3.Equals("1");
                    addDiscs = pref.Val4.Equals("1");
                    logger.Debug("Pref 1021 vals: addComps=" + addComps + " addVoids=" + addVoids + " addDiscs=" + addDiscs);
                }
            }
            for (int x = -1; x >= -14; x--)
            {
                try
                {
                    nextDate = nextDate.AddDays(-1);

                    String dateStr = nextDate.ToString("yyyyMMdd");
                    String fileName = "JRN" + dateStr.Substring(0, 5) + "." + dateStr.Substring(5);

                    logger.Debug(dateStr + ":" + fileName);
                    try
                    {
                        File.Copy(@"C:\network\touchit\jor2\" + fileName, System.Windows.Forms.Application.StartupPath + "\\" + fileName, true);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Could not find file in main folder", ex);
                    }
                    try
                    {
                        //text = File.OpenText( System.Windows.Forms.Application.StartupPath + "\\"+dateStr+".CSV" );
                        using (FileStream fs = File.OpenRead(System.Windows.Forms.Application.StartupPath + @"\" + fileName))
                        {
                            BinaryReader br = new BinaryReader(fs, System.Text.Encoding.ASCII);
                            Hashtable transIds = new Hashtable();
                            double tax = 0;
                            double total = 0;
                            int count = 0;
                            int transId = 0;
                            int status = 0;
                            int voidId = 0;
                            byte sluff;
                            SalesItem item = null;
                            while (br.PeekChar() > 0)
                            {
                                try
                                {
                                    char ch = br.ReadChar();

                                    //ALWAYS MAKE SURE YOU TAKE CARE OF ALL 53 REMAINING bytes in this "row"
                                    switch (ch)
                                    {
                                        case 'A'://HEAD Row.  Begin Record

                                            sluff = br.ReadByte();//empty byte
                                            item = new SalesItem();
                                            br.Read(new byte[6], 0, 6);
                                            transId = br.ReadInt16();
                                            br.Read(new byte[4], 0, 4);
                                            String time = br.ReadInt16().ToString();
                                            status = br.ReadInt16();
                                            voidId = br.ReadInt16();
                                            int hour = 0;
                                            int minute = 0;
                                            if (time.Length > 2)
                                            {
                                                if (time.Length == 4)
                                                    hour = Convert.ToInt32(time.Substring(0, 2));
                                                else
                                                    hour = Convert.ToInt32(time.Substring(0, 1));
                                                if (time.Length == 4)
                                                    minute = Convert.ToInt32(time.Substring(2, 2));
                                                else if (time.Length == 3)
                                                    minute = Convert.ToInt32(time.Substring(1, 2));
                                                else
                                                    minute = 0;
                                            }
                                            else if (time.Length <= 2)//midnight sales items hve no hour in front of them, due to trimming leading zeros
                                            {
                                                hour = 0;
                                                minute = Convert.ToInt32(time);
                                            }
                                            HsDateTime dateTime = new HsDateTime(Convert.ToInt32(dateStr.Substring(6, 2))
                                                , Convert.ToInt32(dateStr.Substring(4, 2))
                                                , Convert.ToInt32(dateStr.Substring(0, 4))
                                                , Convert.ToInt32(hour)
                                                , Convert.ToInt32(minute));
                                            DateTime dt = new DateTime(Convert.ToInt32(dateStr.Substring(0, 4)), Convert.ToInt32(dateStr.Substring(4, 2)), Convert.ToInt32(dateStr.Substring(6, 2))
                                                , Convert.ToInt32(hour), Convert.ToInt32(minute), 0);
                                            if (dateTime.Hour < 6)
                                            {
                                                dt = dt.AddDays(1);
                                            }
                                            item.DayOfMonth = dt.Day;
                                            item.Hour = dt.Hour;
                                            item.Minute = dt.Minute;
                                            item.Month = dt.Month;
                                            item.Year = dt.Year;
                                            item.ExtId = Convert.ToInt64((dateStr) + (count++));
                                            tax = 0;
                                            total = 0;
                                            br.Read(new byte[34], 0, 34);//finish off the row
                                            break;
                                        case 'B'://Sales Row
                                            sluff = br.ReadByte();//empty byte
                                            br.Read(new byte[8], 0, 8);
                                            int dept = br.ReadInt16();
                                            br.Read(new byte[2], 0, 2);
                                            String amt = br.ReadInt32().ToString();
                                            bool pos = amt.IndexOf("-") == -1;
                                            if (!pos)
                                            {
                                                amt = amt.Replace("-", "");
                                            }
                                            int len = amt.Length;
                                            if (validDept(dept))
                                            {
                                                if (len > 1)
                                                {
                                                    StringBuilder sb = new StringBuilder(len + 1);
                                                    sb.Append(amt.Substring(0, len - 2) + "." + amt.Substring(len - 2, 2));
                                                    total += Convert.ToDouble(sb.ToString()) * (pos ? 1 : -1);
                                                }
                                                if (len == 1)
                                                {
                                                    String t = "0.0" + amt;
                                                    total += Convert.ToDouble(t.ToString()) * (pos ? 1 : -1);
                                                }
                                            }
                                            br.Read(new byte[36], 0, 36);//finish off the row
                                            break;
                                        case 'C'://Void Row
                                            sluff = br.ReadByte();//empty byte
                                            br.Read(new byte[8], 0, 8);
                                            int vdept = br.ReadInt16();
                                            br.Read(new byte[2], 0, 2);
                                            String vamt = br.ReadInt32().ToString();
                                            bool vpos = vamt.IndexOf("-") == -1;
                                            if (!vpos)
                                            {
                                                vamt = vamt.Replace("-", "");
                                            }
                                            int vlen = vamt.Length;

                                            if (validDept(vdept) && !addVoids)
                                            {
                                                if (vlen > 1)
                                                {
                                                    StringBuilder vsb = new StringBuilder(vlen + 1);
                                                    vsb.Append(vamt.Substring(0, vlen - 2) + "." + vamt.Substring(vlen - 2, 2));
                                                    total -= Convert.ToDouble(vsb.ToString()) * (vpos ? 1 : -1);
                                                }
                                                if (vlen == 1)
                                                {
                                                    String t = "0.0" + vamt;
                                                    total -= Convert.ToDouble(t.ToString()) * (vpos ? 1 : -1);
                                                }
                                            }
                                            br.Read(new byte[36], 0, 36);//finish off the row
                                            break;
                                        case 'D'://Comp Row
                                            sluff = br.ReadByte();//empty byte
                                            br.Read(new byte[8], 0, 8);
                                            int cdept = br.ReadInt16();
                                            br.Read(new byte[2], 0, 2);
                                            String camt = br.ReadInt32().ToString();
                                            bool cpos = camt.IndexOf("-") == -1;
                                            if (!cpos)
                                            {
                                                camt = camt.Replace("-", "");
                                            }
                                            int clen = camt.Length;
                                            if (validDept(cdept) && !addComps)
                                            {
                                                if (clen > 1)
                                                {
                                                    StringBuilder csb = new StringBuilder(clen + 1);
                                                    csb.Append(camt.Substring(0, clen - 2) + "." + camt.Substring(clen - 2, 2));
                                                    total -= Convert.ToDouble(csb.ToString()) * (cpos ? 1 : -1);
                                                }
                                                if (clen == 1)
                                                {
                                                    String t = "0.0" + camt;
                                                    total -= Convert.ToDouble(t.ToString()) * (cpos ? 1 : -1);
                                                }
                                            }
                                            br.Read(new byte[36], 0, 36);//finish off the row
                                            break;
                                        case 'E'://Disc Row
                                            sluff = br.ReadByte();//empty byte
                                            br.Read(new byte[8], 0, 8);
                                            int ddept = br.ReadInt16();
                                            br.Read(new byte[2], 0, 2);
                                            String damt = br.ReadInt32().ToString();
                                            bool dpos = damt.IndexOf("-") == -1;
                                            if (!dpos)
                                            {
                                                damt = damt.Replace("-", "");
                                            }
                                            int dlen = damt.Length;
                                            if (validDept(ddept) && !addDiscs)
                                            {
                                                if (dlen > 1)
                                                {
                                                    StringBuilder dsb = new StringBuilder(dlen + 1);
                                                    dsb.Append(damt.Substring(0, dlen - 2) + "." + damt.Substring(dlen - 2, 2));
                                                    total -= Convert.ToDouble(dsb.ToString()) * (dpos ? 1 : -1);
                                                }
                                                if (dlen == 1)
                                                {
                                                    String t = "0.0" + damt;
                                                    total -= Convert.ToDouble(t.ToString()) * (dpos ? 1 : -1);
                                                }
                                            }
                                            br.Read(new byte[36], 0, 36);//finish off the row
                                            break;
                                        case 'K'://Tail Row.  End record
                                            item.Amount = total;



                                            if (item.Year > 2007 && voidId == 0 && status == 0 && item.Amount != 0)
                                            {
                                                this.Add(item);
                                                transIds.Add(transId, item);
                                            }
                                            br.Read(new byte[54], 0, 53);//skip the row, we don't need it
                                            break;
                                        default:
                                            byte[] tempest = new byte[54];
                                            br.Read(tempest, 0, 53);//skip the row, we don't need it
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error reading char", ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in file: " + System.Windows.Forms.Application.StartupPath + @"\" + fileName, ex);
                    }
                    if (File.Exists(System.Windows.Forms.Application.StartupPath + "\\" + fileName))
                    {
                        File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + fileName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("missing day: " + nextDate.ToString("yyyyMMdd"));
                    logger.Error(ex.ToString());
                }
            }
        }
        /*
		public override void DbLoad(  )
		{
			StreamReader text = null;
			DateTime nextDate = DateTime.Today;
			logger.Debug("before forLoop:  " + nextDate.ToString("yyyyMMdd"));
			for(int x = -1; x >= -13; x--)
			{
				try
				{
					nextDate = nextDate.AddDays(-1);

					String dateStr = nextDate.ToString("yyyyMMdd");
					String fileName = "JRN" + dateStr.Substring(0, 5) + "." + dateStr.Substring(5);

					logger.Debug(dateStr+":"+fileName);
					try
					{
						File.Copy( @"C:\network\touchit\jor2\"+fileName , System.Windows.Forms.Application.StartupPath + "\\"+fileName , true);
					}
					catch(Exception ex)
					{
						logger.Debug("Could not find file in main filder:  " + ex.StackTrace);

					}
					try
					{
						//text = File.OpenText( System.Windows.Forms.Application.StartupPath + "\\"+dateStr+".CSV" );
						using (FileStream fs = File.OpenRead(System.Windows.Forms.Application.StartupPath + @"\"+fileName))
						{
							BinaryReader br = new BinaryReader(fs, System.Text.Encoding.ASCII);
							Hashtable transIds = new Hashtable();
							double tax = 0;
							double total = 0;
							int count = 0;
							int transId = 0;
							int status = 0;
							int voidId = 0;
							byte sluff;
							SalesItem item = null;
							while(br.PeekChar() > 0)
							{
								char ch = br.ReadChar();
								
								//ALWAYS MAKE SURE YOU TAKE CARE OF ALL 53 REMAINING bytes in this "row"
								switch(ch)
								{
									case 'A'://HEAD Row.  Begin Record
										
										sluff = br.ReadByte();//empty byte
										item = new SalesItem();
										br.Read(new byte[6], 0, 6);
										transId = br.ReadInt16();
										br.Read(new byte[4], 0, 4);
										String time = br.ReadInt16().ToString();
										status = br.ReadInt16();
										voidId = br.ReadInt16();
										int hour =  0;
										int minute = 0;
										if(time.Length == 4)
											hour = Convert.ToInt32(time.Substring(0,2));
										else
											hour = Convert.ToInt32(time.Substring(0, 1));
										if(time.Length == 4)
											minute = Convert.ToInt32(time.Substring(2,2));
										else if(time.Length == 3)
											minute = Convert.ToInt32(time.Substring(1, 2));
										else
											minute = 0;
										HsDateTime dateTime = new HsDateTime(Convert.ToInt32(dateStr.Substring(6,2))
											, Convert.ToInt32(dateStr.Substring(4,2))
											, Convert.ToInt32(dateStr.Substring(0, 4))
											, Convert.ToInt32(hour)
											, Convert.ToInt32(minute));
										DateTime dt = new DateTime(Convert.ToInt32(dateStr.Substring(0, 4)), Convert.ToInt32(dateStr.Substring(4,2)), Convert.ToInt32(dateStr.Substring(6,2))
											, Convert.ToInt32(hour)	, Convert.ToInt32(minute), 0);
										if(dateTime.Hour < 6)
										{
											dt = dt.AddDays(1);
										}
										item.DayOfMonth = dt.Day;
										item.Hour = dt.Hour;
										item.Minute = dt.Minute;
										item.Month = dt.Month;
										item.Year = dt.Year;
										item.ExtId = Convert.ToInt64((dateStr) + (count++));
										tax = 0;
										total = 0;
										br.Read(new byte[34], 0, 34);//finish off the row
										break;
									case 'B'://Sales Row
										sluff = br.ReadByte();//empty byte
										br.Read(new byte[8], 0, 8);
										int dept = br.ReadInt16();
										br.Read(new byte[2], 0, 2);
										String amt = br.ReadInt32().ToString();
										int len = amt.Length;
										if(dept != 10)
										{
											if(len > 1)
											{
												StringBuilder sb = new StringBuilder(len+1);
												sb.Append(amt.Substring(0, len-2) + "." + amt.Substring(len-2, 2));
												total += Convert.ToDouble(sb.ToString());
											}
											if(len == 1)
											{
												String t = "0.0" + amt;
												total += Convert.ToDouble(t.ToString());
											}
										}
										br.Read(new byte[36], 0, 36);//finish off the row
										break;
									case 'C'://Void Row
										sluff = br.ReadByte();//empty byte
										br.Read(new byte[8], 0, 8);
										int vdept = br.ReadInt16();
										br.Read(new byte[2], 0, 2);
										String vamt = br.ReadInt32().ToString();
										int vlen = vamt.Length;
										if(vdept != 10)
										{
											if(vlen > 1)
											{
												StringBuilder vsb = new StringBuilder(vlen+1);
												vsb.Append(vamt.Substring(0, vlen-2) + "." + vamt.Substring(vlen-2, 2));
												total -= Convert.ToDouble(vsb.ToString());
											}
											if(vlen == 1)
											{
												String t = "0.0" + vamt;
												total -= Convert.ToDouble(t.ToString());
											}
										}
										br.Read(new byte[36], 0, 36);//finish off the row
										break;
									case 'K'://Tail Row.  End record
										item.Amount = total;
										

										
										if(item.Year > 2007 && voidId ==0 && status == 0 && item.Amount != 0)
										{
											this.Add(item);
											transIds.Add(transId, item);
										}
										br.Read(new byte[54], 0, 53);//skip the row, we don't need it
										break;
									default:
										br.Read(new byte[54], 0, 53);//skip the row, we don't need it
										break;
								}
							}
						}
					}
					catch(Exception ex)
					{
						logger.Error(ex.ToString());
					}
					if( File.Exists( System.Windows.Forms.Application.StartupPath + "\\"+fileName ) )
					{
						File.Delete( System.Windows.Forms.Application.StartupPath + "\\"+fileName );
					}
				}
				catch(Exception ex)
				{
					logger.Error("missing day: " + nextDate.ToString("yyyyMMdd"));
					logger.Error(ex.ToString());
				}
			}
		}*/

		private String[] LineToArray( String line )
		{
			int startIndex = 0;
			int length = 0;
			ArrayList strings = new ArrayList();
			while( startIndex < line.Length )
			{
				if( line.Substring( startIndex, 1 ).CompareTo( "\"" ) == 0 )
				{
					length = line.IndexOf( "\"," ,startIndex+1 ) - (startIndex+1);
					length = length < 0 ? (line.Length-1) - (startIndex+1) : length;
					String s = line.Substring( startIndex+1, length );
					strings.Add( s );
					startIndex += (length + 3);
				}
				else
				{
					length = line.IndexOf( "," ,startIndex+1 ) - startIndex;
					length = length < 0 ? line.Length - startIndex : length;
					String s = line.Substring( startIndex, length );
					strings.Add( s );
					startIndex += (length + 1);
				}
			}
			String[] strs = new String[strings.Count];			
			int cnt = 0;
			foreach( String str in strings )
			{
				strs[cnt] = str.Trim();
				cnt++;	
			}
			return strs;
		}

		public override void DbUpdate(){}
		public override void DbInsert(){}

		private void JrnToCsv(String date)
		{
			try
			{
				System.Diagnostics.Process proc = new System.Diagnostics.Process();
				proc.EnableRaisingEvents=false;
				proc.StartInfo.WorkingDirectory = @"G:\APPLEBEE\BIN";
				proc.StartInfo.FileName="JRNTOCSV.EXE";
				proc.StartInfo.Arguments=@"/date=" + date + @" /activeonly";
				logger.Debug( "running start" );
				logger.Debug(proc.StartInfo.Arguments.ToString());
				proc.Start();
				logger.Debug( "ran start" );
				proc.WaitForExit();
				logger.Debug( "exited process" );
			}
			catch(Exception ex)
			{
				logger.Error("Error running jrntocsv.exe", ex);
			}
		}

        private bool validDept(int dept)
        {
            try
            {
                if (dept < 10)
                {
                    return true;
                }
                if (dept > 10)
                {
                    String s = dept + "";
                    if (s.Length >= 4)
                        return (Int32.Parse(s.Substring(0, 2)) != 10);
                    else
                        return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error getting dept code", ex);
            }
            return false;
        }
	}
}
