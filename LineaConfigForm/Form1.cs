using NLog;
using LineaConfiguration ;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace LineaConfigForm
{
    public partial class LineaConfig : Form
    {
        public LineaConfig()
        {
            InitializeComponent();
        }
        Configuration LineaConfiguration { get; set; }
        Configuration UpdateConfiguration { get; set; }
        Configuration WordConfiguration { get; set; }

        List<Button> buttonList = new List<Button>();

        string fileName { get; set; }
        string filePath { get; set; }

        string fileName2 { get; set; }
        string filePath2 { get; set; }

        private void btParam_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog() { Filter = "Fichier LBL|param.lbl" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LineaConfiguration = new Configuration(dialog.FileName, 1);

                string block = "";
                foreach (var line in LineaConfiguration.ParamText)
                {
                     block += line + "\r\n";
                }
                txtFile.Text = block;
                txtParam.Text = dialog.FileName;
                btParamU.Enabled = true;
                btParam.Enabled = false;
            }
        }

        private void btParamU_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog() { Filter = "Fichier LBL|param_u.lbl" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                UpdateConfiguration = new Configuration(dialog.FileName, 2);

                fileName = dialog.FileName;
                filePath = dialog.SafeFileName;

                string block = "";
                foreach (var line in UpdateConfiguration.ParamUText)
                {
                    block += line + "\r\n";
                }
                txtFile.Text = block;
                txtParamU.Text = dialog.FileName;
                btConfig.Enabled = true;
                btParamU.Enabled = false;

                ParamCompare();
            }
        }

        private void ParamCompare()
        {
            UpdateConfiguration.Compare(LineaConfiguration);
        }

        private void btConfig_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog() { Filter = "Fichier Config|linea_cfg.config" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                WordConfiguration = new Configuration(dialog.FileName, 3);

                fileName2 = dialog.FileName;
                filePath2 = dialog.SafeFileName;

                string block = "";
                foreach (var line in WordConfiguration.ConfigurationText)
                {
                    block += line + "\r\n";
                }
                txtFile.Text = block;
                txtConfig.Text = dialog.FileName;
                txtSearch.Enabled = btDL.Enabled = true;
                btConfig.Enabled = false;

                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            dgvContent.Rows.Clear();

            var properties = typeof(Param).GetProperties().OrderBy(p => p.MetadataToken);
            var dictionnary = WordConfiguration.Trads;

            int y = 0;

            dgvContent.ColumnCount = properties.Count() - 1;
            foreach (var property in properties)
            {
                if (property.Name != "Wording")
                    dgvContent.Columns[y].Name = property.Name;
                y++;
            }

            foreach (var n in LineaConfiguration.Params)
            {
                string[] row = new string[] { n.Value.Key, n.Value.Origin, n.Value.Wording };
                dgvContent.Rows.Add(row);
            }

            dgvText.Rows.Clear();

            int x = 0;

            dgvText.ColumnCount = properties.Count() - 2;
            foreach (var property in properties)
            {
                if (property.Name != "Wording" && property.Name != "Origin")
                    dgvText.Columns[x].Name = property.Name;
                x++;
            }

            foreach (var n in LineaConfiguration.Params)
            {
                string[] row = new string[] { n.Value.Key };
                dgvText.Rows.Add(row);
            }


            dgvTrad.Rows.Clear();

            var properties2 = typeof(Serialize).GetProperties().OrderBy(p => p.MetadataToken);

            int z = 0;

            dgvTrad.ColumnCount = properties2.Count();
            foreach (var property in properties2)
            {
                dgvTrad.Columns[z].Name = property.Name;
                z++;
            }

            if (WordConfiguration.Trads != null)
            {
                foreach (var n in WordConfiguration.Trads)
                {
                    string[] row = new string[] { n.Value.Name, n.Value.Description };
                    dgvTrad.Rows.Add(row);
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            dgvContent.Rows.Clear();

            var properties = typeof(Param).GetProperties().OrderBy(p => p.MetadataToken);

            int y = 0;

            dgvContent.ColumnCount = properties.Count() - 1;
            foreach (var property in properties)
            {
                if (property.Name != "Wording")
                    dgvContent.Columns[y].Name = property.Name;
                y++;
            }

            foreach (var n in LineaConfiguration.Params)
            {
                if (n.Value.Key.Contains(txtSearch.Text.ToUpper()))
                {
                    string[] row = new string[] { n.Value.Key, n.Value.Origin, n.Value.Wording };
                    dgvContent.Rows.Add(row);
                }
            }
        }

        private void addBox(string content)
        {
            TextBox textbox = new TextBox();
            int count = panel1.Controls.OfType<TextBox>().ToList().Count;
            textbox.Location = new System.Drawing.Point(10, 25 * count);
            textbox.Size = new System.Drawing.Size(250, 20);
            textbox.Name = "txt_" + (count + 1);
            textbox.Text = content;
            panel1.Controls.Add(textbox);
        }

        private void dgvContent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            panel1.Controls.Clear();
            var current = dgvContent.SelectedCells[0].Value.ToString();
            var selected = LineaConfiguration.Params[current].Wording;
            var words = WordConfiguration.Split(selected);

            txtName.Text = words[0];
            txtLine.Text = selected;

            var i = 1;
            while (i < words.Length)
            {
                addBox(words[i]);
                i++;
            }

            btSave.Enabled = true;
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            var textList = panel1.Controls.OfType<TextBox>().OrderBy(tb => tb.Left).Select(tb => tb.Text).ToList();

            var line = WordConfiguration.SaveLine(txtName.Text, textList);

            var words = txtName.Text.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            UpdateConfiguration.ReplaceUpdate(line, words[1]);
        }

        private void btDL_Click(object sender, EventArgs e)
        {
            var path = fileName.Replace(filePath, "param_u.lbl");
            var a = UpdateConfiguration.WriteText();
            string[] text = (string[])UpdateConfiguration.WriteText();
            File.WriteAllLines(path, text);
            txtDL.Text += "File saved in : " + path;
            btDL.Enabled = false;

            //var b = WordConfiguration.ConfigurationText;
            ////var serialization = JsonConvert.SerializeObject(a, Formatting.Indented);
            //File.WriteAllText(fileName, serialization);

            string block = "";
            foreach (var line in UpdateConfiguration.ParamUText)
            {
                block += line + "\r\n";
            }
            txtFile.Text = block;
        }

        private void dgvText_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ClearTrad();

            var current = dgvText.SelectedCells[0].Value.ToString();
            var dictionnary = WordConfiguration.Trads;

            if (!dictionnary.ContainsKey(current))
            {
                txtInitial.Text = current;
            }
            else
            {
                txtInitial.Text = dictionnary[current].Name;
                txtMod.Text = dictionnary[current].Description;
            }

            btMod.Enabled = txtMod.Enabled = true;
        }

        private void dgvTrad_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ClearTrad();

            var current = dgvTrad.SelectedCells[0].Value.ToString();
            var dictionnary = WordConfiguration.Trads;

            txtInitial.Text = dictionnary[current].Name;
            txtMod.Text = dictionnary[current].Description;

            btMod.Enabled = txtMod.Enabled = true;
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            btMod.Enabled = txtMod.Enabled = false;

            dgvText.ClearSelection();
            dgvTrad.ClearSelection();

            txtInitial.Clear();
            txtMod.Clear();
        }

        private void btMod_Click(object sender, EventArgs e)
        {
            var initial = txtInitial.Text;
            var mod = txtMod.Text;
            string? serialization = "";

            if (String.IsNullOrEmpty(txtMod.Text))
                MessageBox.Show("Please complete the fields", "ERROR");
            else
            {
                WordConfiguration.AddTrad(initial, mod);

                foreach (var trad in WordConfiguration.Trads)
                {
                    serialization += JsonConvert.SerializeObject(trad, Formatting.Indented);
                }
            }
            var path = fileName2.Replace(filePath2, "linea_cfg.config");
            File.WriteAllText(path, serialization);
            txtLog.Text += "File saved in : " + path;

            ClearTrad();
            UpdateDisplay();
        }

        private void ClearTrad()
        {
            btMod.Enabled = txtMod.Enabled = false;

            txtInitial.Clear();
            txtMod.Clear();
        }
    }
}