using System;
using System.Collections;
using System.IO;
using System.Text;
using HsSharedObjects.Main;

namespace HsSharedObjects
{
    public class MapFile
    {
        private readonly Hashtable _hash = new Hashtable();
        private readonly string _filePath;
        private readonly SysLog logger = new SysLog(typeof(MapFile));

        public MapFile(String filePath, bool load)
        {
            _filePath = filePath;
            if(load)
                Load();
        }

        public String this[String key]
        {
            get {
                return Get(key);
            }
            set
            {
                Add(key, value);
            }
        }

        private string Get(string key)
        {
            object val = _hash[key.ToLower()];
            if (val != null)
                return val.ToString();
            return null;
        }

        public void Add(string key, string value)
        {
            _hash.Add(key.ToLower(), value);
        }

        public void AddNext(string key)
        {
            _hash.Add(key.ToLower(), MaxVal + 1);
        }

        public bool Load()
        {
            logger.Debug("Loading Map File");
            logger.Debug("file = " + _filePath);
            try
            {
                if (File.Exists(_filePath))
                {
                    TextReader reader = null;
                    try
                    {
                        reader = new StreamReader(_filePath);
                        String line;
                        while((line=reader.ReadLine())!=null)
                        {
                            try
                            {
                                String key = line.Split('|')[0].ToLower();
                                String val = line.Split('|')[1];
                                Add(key, val);
                            }
                            catch
                            {
                                logger.Error("Error reading line: " + line);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error reading file");
                        logger.Error(ex.ToString());
                        return false;
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                }
                logger.Debug("Successfully loaded Map file");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Error loading Map file");
                logger.Error(ex.ToString());
                return false;
            }
        }

        public bool Save()
        {
            try
            {
                logger.Debug("Saving Map file");
                logger.Debug("path = " + _filePath);

                CheckFileExistence(_filePath);

                TextWriter writer = null;
                try
                {
                    writer = new StreamWriter(_filePath);

                    foreach (Object o in _hash.Keys)
                    {
                        String key = o.ToString().ToLower();
                        String val = this[o.ToString()];
                        writer.WriteLine(key + "|" + val);
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
                logger.Debug("Successfully saved Map file");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Error saving Map file");
                logger.Error(ex.ToString());
                return false;
            }
        }

        public String GetKeyByValue(String value)
        {
            foreach(Object o in _hash.Keys)
            {
                String key = o.ToString().ToLower();
                String val = Get(key);
                if(val.Equals(value))
                    return key;
            }
            return null;
        }

        public void SetHash(Hashtable hashtable)
        {
            Clear();

            foreach(Object o in hashtable)
                Add(o.ToString().ToLower(), hashtable[o].ToString());
        }

        public int MaxVal
        {
            get
            {
                int max = 0;
                foreach(Object v in _hash.Values)
                {
                    int val = Int32.Parse(v.ToString());
                    if(val>max)
                        max = val;
                }
                return max;
            }
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        private static void CheckFileExistence(String filePath)
        {
            String dir = filePath.Substring(0, filePath.LastIndexOf('\\'));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public bool ContainsKey(String key)
        {
            return _hash.ContainsKey(key.ToLower());
        }

        public void Clear()
        {
            _hash.Clear();
        }

        public int Count
        {
            get { return _hash.Count; }
        }
    }
}
