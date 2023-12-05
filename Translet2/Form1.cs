using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Translet2
{
    public partial class Form1 : Form
    {
        private readonly StringBuilder sourceCodeBuilder;
        private string selectedJsonFilePath;
        public Form1()
        {
            InitializeComponent();
            sourceCodeBuilder = new StringBuilder();
        }

        private void GeneratePhpCode(string jsonFilePath)
        {
            // Read JSON file content
            string umlDiagramJson = File.ReadAllText(jsonFilePath);

            // Decode JSON data
            JsonData json = JsonConvert.DeserializeObject<JsonData>(umlDiagramJson);

            // Generate PHP code
            GenerateNamespace(json.sub_name);

            foreach (var model in json.model)
            {
                if (model.type == "class")
                {
                    GenerateClass(model);
                }
                else if (model.type == "association" && model.model != null)
                {
                    GenerateAssociationClass(model.model);
                }
            }

            // Display or save the generated PHP code
            richTextBox2.Text = sourceCodeBuilder.ToString();
        }

        private void GenerateNamespace(string namespaceName)
        {
            sourceCodeBuilder.AppendLine($"<?php\nnamespace {namespaceName}\n");
        }

        private void GenerateClass(JsonData.Model model)
        {
            sourceCodeBuilder.AppendLine($"class {model.class_name} {{");

            foreach (var attribute in model.attributes)
            {
                GenerateAttribute(attribute);
            }

            sourceCodeBuilder.AppendLine("");

            foreach (var status in model.attributes)
            {
                GenerateState(status);
            }

            sourceCodeBuilder.AppendLine("");

            if (model.states != null)
            {
                foreach (var state in model.states)
                {
                    GenerateStateTransitionMethod(state);
                }
            }

            if (model.states != null)
            {
                GenerateGetState();
            }


            sourceCodeBuilder.AppendLine("}\n\n");
        }

        private void GenerateAttribute(JsonData.Attribute1 attribute)
        {
            // Adjust data types as needed
            string dataType = MapDataType(attribute.data_type);

            if (dataType != "state") 
            {
                sourceCodeBuilder.AppendLine($"    private {dataType} ${attribute.attribute_name};");
            }
        }

        private void GenerateAssociationClass(JsonData.Model associationModel)
        {
            // Check if associationModel is not null
            if (associationModel == null)
            {
                // Handle the case where associationModel is null, e.g., throw an exception or log a message
                return;
            }

            sourceCodeBuilder.AppendLine($"class {associationModel.class_name} {{");

            foreach (var attribute in associationModel.attributes)
            {
                // Adjust data types as needed
                string dataType = MapDataType(attribute.data_type);

                sourceCodeBuilder.AppendLine($"    private {dataType} ${attribute.attribute_name};");
            }

            // Check if associatedClass.@class is not null before iterating
            if (associationModel.@class != null)
            {
                foreach (var associatedClass in associationModel.@class)
                {
                    if (associatedClass.class_multiplicity == "1..1")
                    {
                        sourceCodeBuilder.AppendLine($"    private {associatedClass.class_name} ${associatedClass.class_name};");
                    }
                    else
                    {
                        sourceCodeBuilder.AppendLine($"    private array ${associatedClass.class_name}List;");
                    }
                }
            }

            sourceCodeBuilder.AppendLine("}\n\n");
        }

        private void GenerateState(JsonData.Attribute1 status) 
        {
            if (status.attribute_name == "status")
            {
                sourceCodeBuilder.AppendLine("     private $state;");
            }
        }
        
        private void GenerateStateTransitionMethod(JsonData.State state)
        {
            sourceCodeBuilder.AppendLine($"     public function {state.state_name}() {{");
            sourceCodeBuilder.AppendLine($"       $this->state = '{state.state_name}';");
            sourceCodeBuilder.AppendLine($"       echo \"state berubah menjadi {state.state_name}\";");
            sourceCodeBuilder.AppendLine($"}}\n");
        }

        private void GenerateGetState()
        {
            sourceCodeBuilder.AppendLine($"     public function GetState() {{");
            sourceCodeBuilder.AppendLine($"       $this->state;");
            sourceCodeBuilder.AppendLine($"}}\n");
        }
        public class JsonData
        {
            public string sub_name { get; set; }
            public List<Model> model { get; set; }

            public class Model
            {
                public string type { get; set; }
                public string class_name { get; set; }
                public List<Attribute1> attributes { get; set; }
                public List<State> states { get; set; }
                public Model model { get; set; }
                public List<Class1> @class { get; set; }
            }

            public class Attribute1
            {
                public string attribute_name { get; set; }
                public string data_type { get; set; }
                public string default_value { get; set; }
                public string attribute_type { get; set; }
            }

            public class State
            {
                public string state_name { get; set; }
            }

            public class Class1
            {
                public string class_name { get; set; }
                public string class_multiplicity { get; set; }
                public List<Attribute> attributes { get; set; }
                public List<Class1> @class { get; set; }
            }

            public class Attribute
            {
                public string attribute_name { get; set; }
                public string data_type { get; set; }
                public string attribute_type { get; set; }
            }
        }

        private string MapDataType(string dataType)
        {
            switch (dataType.ToLower())
            {
                case "integer":
                    return "int";
                case "id":
                    return "int";
                case "string":
                    return "string";
                case "bool":
                    return "bool";
                case "real":
                    return "float";
                // Add more mappings as needed
                default:
                    return dataType; // For unknown types, just pass through
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open Json Diagram File";
            dialog.Filter = "Json Diagram Files|*.json";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                selectedJsonFilePath = dialog.FileName;
                string displayJson = File.ReadAllText(selectedJsonFilePath);
                tabControl1.SelectTab(tabPage1);
                richTextBox1.Text = displayJson;
            }
        }

        private void btn_translate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(selectedJsonFilePath) && File.Exists(selectedJsonFilePath))
                {
                    GeneratePhpCode(selectedJsonFilePath);
                }
                else
                {
                    MessageBox.Show("Please select a valid JSON file first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PHP code: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
