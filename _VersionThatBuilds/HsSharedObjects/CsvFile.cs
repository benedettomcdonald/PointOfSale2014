using System;
using System.Collections;
using System.IO;
using System.Text;
using HsSharedObjects.Main;

namespace HsSharedObjects
{
    public class CsvFile
    {
        private ArrayList _values = new ArrayList();
        private readonly char _delimiter;
        private readonly SysLog logger = new SysLog(typeof(CsvFile));

        public CsvFile()
        {
            _delimiter = '|';
        }

        public CsvFile(char delimiter)
        {
            _delimiter = delimiter;
        }

        public ArrayList Values
        {
            get { return _values; }
            set { _values = value; }
        }

        public void AddRow(ArrayList row)
        {
            _values.Add(row);
        }

        public bool SaveAs(string filePath)
        {
            try
            {
                logger.Debug("Saving CSV File");
                logger.Debug("path = " + filePath);

                CheckFileExistence(filePath);

                TextWriter writer = null;
                try
                {
                    writer = new StreamWriter(filePath);
                    foreach (ArrayList row in _values)
                    {
                        foreach (Object o in row)
                        {
                            String cell = o==null?"":o.ToString();
                            writer.Write(cell + _delimiter);
                        }
                        writer.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error writing to file");
                    logger.Error(ex.ToString());
                    return false;
                }
                finally
                {
                    if (writer != null)
                        writer.Close();
                }
                logger.Debug("Successfully saved CSV file");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Error saving CSV File");
                logger.Error(ex.ToString());
                return false;
            }
        }

        private static void CheckFileExistence(string filePath)
        {
            String dir = filePath.Substring(0, filePath.LastIndexOf('\\'));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
