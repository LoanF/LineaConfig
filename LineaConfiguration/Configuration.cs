using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace LineaConfiguration
{
    public class Configuration
    {
        /// <summary>
        /// Logger de la classe Configuration
        /// </summary>
        private static readonly Logger appLog = LogManager.GetLogger("ConfigurationLogger");

        public List<string> ParamText { get; set; }
        public List<string> ParamUText { get; set; }
        public List<string> ConfigurationText { get; set; }
        private int Pointer { get; set; }


        public Dictionary<string, Param> Params { get; set; }
        public Dictionary<string, Serialize> Trads { get; set; }

        public Configuration(string pathName, int nbButton)
        {
            appLog.Info($"Fichier : {pathName}");

            if (nbButton == 1)
            {
                ParamText = (File.ReadAllLines(pathName)).ToList();
                SeekStarting();
                Params = GetParams(nbButton);
            }
            else if (nbButton == 2)
            {
                ParamUText = (File.ReadAllLines(pathName)).ToList();
                SeekStarting();
                Params = GetParams(nbButton);
            }
            else if (nbButton == 3)
            {
                ConfigurationText = (File.ReadAllLines(pathName)).ToList();
                SeekStarting();
                Trads = GetTrads();
            }
        }

        private Dictionary<string, Serialize>? GetTrads()
        {
            var tradz = new Dictionary<string, Serialize>();

            Serialize newTrad;
            while ((newTrad = GetNextTrad()).Name != null)
            {
                if(!tradz.ContainsKey(newTrad.Name))
                {
                    tradz[newTrad.Name] = newTrad;
                }
            }

            return tradz;
        }

        private Serialize GetNextTrad()
        {
            var p = Pointer;
            var newTrad = new Serialize();
            bool done = false;
            while (p < ConfigurationText.Count && done == false)
            {
                var words = ConfigurationText[p].Split(new char[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                {
                    if (words[1] == "Name")
                    {
                        newTrad.Name = words[3];
                    }
                    
                    if (words[1] == "Description")
                    {
                        newTrad.Description = words[3];

                        done = true;
                    }
                }

                p++;
            }
            Pointer = p;

            return newTrad;
        }

        private void SeekStarting()
        {
            Pointer = 0;
        }

        private Dictionary<string, Param> GetParams(int nbButton)
        {
            var paramz = new Dictionary<string, Param>();

            Param newParam;
            while ((newParam = GetNextParam(nbButton)).Key != null)
            {
                if (!paramz.ContainsKey(newParam.Key))
                {
                    paramz[newParam.Key] = newParam;
                }
            }

            return paramz;
        }

        public void Compare(Configuration lineaConfiguration)
        {
            var listConfig = lineaConfiguration.Params;
            var listUpdate = Params;

            foreach (var param in listConfig)
            {
                foreach (var update in listUpdate)
                {
                    if (update.Key == param.Key)
                    {
                        listConfig[param.Key] = update.Value;
                    }
                }
            }
        }

        private Param GetNextParam(int nbButton)
        {
            var p = Pointer;
            var newParam = new Param();
            bool done = false;
            if (nbButton == 1)
            {
                while (p < ParamText.Count && done == false)
                {
                    var words = ParamText[p].Split(new char[] { ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length >= 1)
                    {
                        if (words[0] == "P" || words[0] == "V" || words[0] == "E" || words[0] == "F" || words[0] == "AXE" || words[0] == "POINT" || words[0] == "COTE" || words[0] == "P_POINT" || words[0] == "CC" || words[0] == ".POINT" || words[0] == "+COTE")
                        {
                            newParam.Key = words[1];
                            newParam.Wording = ParamText[p];
                            newParam.Origin = "param.lbl";

                            done = true;
                        }
                    }

                    p++;
                }
                Pointer = p;
            }
            else if(nbButton == 2)
            {
                while (p < ParamUText.Count && done == false)
                {
                    var words = ParamUText[p].Split(new char[] { ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

                    if (words.Length >= 1)
                    {
                        if (words[0] == "P")
                        {
                            newParam.Key = words[1];
                            newParam.Wording = ParamUText[p];
                            newParam.Origin = "param_u.lbl";

                            done = true;
                        }
                    }
                    p++;
                }
                Pointer = p;
            }
            return newParam;
        }

        public void ReplaceUpdate(string line, string key)
        {
            Param updateParam = new Param();
            updateParam.Key = key;
            updateParam.Origin = "param_u.lbl";
            updateParam.Wording = line;

            Params[updateParam.Key] = updateParam;
        }

        public string SaveLine(string name, List<string> textList)
        {
            var line = name;
            foreach (var item in textList)
            {
                line += ";" + item;
            }

            return line;
        }

        public string[] WriteText()
        {
            var wording = new List<string>();

            wording.Add("col0;col1;col2;col3;col4;col5");
            wording.Add("\r\n[PARAMETRES UTILISATEUR]");

            foreach (var paramz in Params)
            {
                wording.Add(paramz.Value.Wording);
            }

            var line = wording.ToArray();

            return line;
        }

        public string[] Split(string value)
        {
            var words = value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            return words;
        }

        public Serialize AddTrad(string initial, string mod)
        {
            Serialize newTrad = new Serialize();

            newTrad.Name = initial;
            newTrad.Description = mod;

            Trads[initial] = newTrad;

            return newTrad;
        }
    }
}