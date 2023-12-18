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

            bool generateAssocClass = json.model.Any(model => model.type == "association");

            if (generateAssocClass)
            {
                sourceCodeBuilder.AppendLine($"// Just an Example");
                GenerateAssocClass();
            }


            foreach (var model in json.model)
            {
                if (model.type == "association")
                {
                    GenerateObjAssociation(model);
                }
            }


            // Display or save the generated PHP code
            richTextBox2.Text = sourceCodeBuilder.ToString();
        }

        private void GenerateNamespace(string namespaceName)
        {
            sourceCodeBuilder.AppendLine($"<?php\nnamespace {namespaceName};\n");
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

            if (model.attributes != null)
            {
                GenerateConstructor(model.attributes);
            }

            sourceCodeBuilder.AppendLine("");

            foreach (var attribute in model.attributes)
            {
                GenerateGetter(attribute);
            }

            sourceCodeBuilder.AppendLine("");

            foreach (var attribute in model.attributes)
            {
                GenerateSetter(attribute);
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

            sourceCodeBuilder.AppendLine($"class assoc_{associationModel.class_name} {{");

            foreach (var attribute in associationModel.attributes)
            {
                // Adjust data types as needed
                string dataType = MapDataType(attribute.data_type);

                sourceCodeBuilder.AppendLine($"     private {dataType} ${attribute.attribute_name};");
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

            sourceCodeBuilder.AppendLine("");

            if (associationModel.attributes != null)
            {
                GenerateConstructor(associationModel.attributes);
            }

            foreach (var attribute in associationModel.attributes)
            {
                GenerateGetter(attribute);
            }

            foreach (var attribute in associationModel.attributes)
            {
                GenerateSetter(attribute);
            }
            sourceCodeBuilder.AppendLine("}\n\n");
        }

        private void GenerateConstructor(List<JsonData.Attribute1> attributes)
        {
            sourceCodeBuilder.Append($"     public function __construct(");

            foreach (var attribute in attributes)
            {
                if(attribute.attribute_name != "status") 
                {
                    sourceCodeBuilder.Append($"${attribute.attribute_name},");
                }
                
            }

            // Remove the trailing comma and add the closing parenthesis
            if (attributes.Any())
            {
                sourceCodeBuilder.Length -= 1; // Remove the last character (",")
            }

            sourceCodeBuilder.AppendLine(") {");

            foreach (var attribute in attributes)
            {
                if (attribute.attribute_name != "status")
                {
                    sourceCodeBuilder.AppendLine($"        $this->{attribute.attribute_name} = ${attribute.attribute_name};");
                }
            }

            // Handle the "status" attribute separately outside the loop
            var statusAttribute = attributes.FirstOrDefault(attr => attr.attribute_name == "status");
            if (statusAttribute != null)
            {
                // Check if the attribute has a default value and it is a string
                if (!string.IsNullOrEmpty(statusAttribute.default_value) && statusAttribute.data_type.ToLower() == "state")
                {
                    int lastDotIndex = statusAttribute.default_value.LastIndexOf('.');
                    // Replace "status" with "state" and "aktif" with "active"
                    string stringValue = statusAttribute.default_value.Substring(lastDotIndex + 1).Replace("aktif", "active");
                    sourceCodeBuilder.AppendLine($"        $this->state = \"{stringValue}\";");
                }
            }

            sourceCodeBuilder.AppendLine("}");
        }

        private void GenerateGetter(JsonData.Attribute1 getter)
        {
            if (getter.attribute_name != "status")
            {
                sourceCodeBuilder.AppendLine($"      public function get{getter.attribute_name}() {{");
                sourceCodeBuilder.AppendLine($"        $this->{getter.attribute_name};");
                sourceCodeBuilder.AppendLine($"}}");
            }
            
        }

        private void GenerateSetter(JsonData.Attribute1 setter)
        {
            if (setter.attribute_name != "status")
            {
                sourceCodeBuilder.AppendLine($"      public function set{setter.attribute_name}(${setter.attribute_name}) {{");
                sourceCodeBuilder.AppendLine($"        $this->{setter.attribute_name} = ${setter.attribute_name};");
                sourceCodeBuilder.AppendLine($"}}");
            }
            
        }
        private void GenerateState(JsonData.Attribute1 status)
        {
            if (status.attribute_name == "status")
            {
                sourceCodeBuilder.AppendLine("    private $state;");
            }
        }

        private void GenerateGetState()
        {
            sourceCodeBuilder.AppendLine($"     public function GetState() {{");
            sourceCodeBuilder.AppendLine($"       $this->state;");
            sourceCodeBuilder.AppendLine($"}}\n");
        }

        private void GenerateStateTransitionMethod(JsonData.State state)
        {
            if (state.state_event != null && state.state_event.Length > 0)
            {
                string setEvent = state.state_event[0];
                string onEvent = state.state_event[1];
                sourceCodeBuilder.AppendLine($"     public function {setEvent}() {{");
                sourceCodeBuilder.AppendLine($"       $this->state = \"{state.state_value}\";");
                sourceCodeBuilder.AppendLine($"}}");

                sourceCodeBuilder.AppendLine($"     public function {onEvent}() {{");
                sourceCodeBuilder.AppendLine($"       echo \"status saat ini {state.state_value}\";");
                sourceCodeBuilder.AppendLine($"}}");
            }


            if (state.state_function != null && state.state_function.Length > 0)
            {
                string setFunction = state.state_function[0];
                sourceCodeBuilder.AppendLine($"     public function {setFunction}() {{");
                sourceCodeBuilder.AppendLine($"       $this->state = \"{state.state_value}\";");
                sourceCodeBuilder.AppendLine($"}}\n");
            }
        }

        private void GenerateAssocClass()
        {
            sourceCodeBuilder.AppendLine($"class Association{{");
            sourceCodeBuilder.AppendLine($"     public function __construct($class1,$class2) {{");
            sourceCodeBuilder.AppendLine($"}}");
            sourceCodeBuilder.AppendLine($"}}");
            sourceCodeBuilder.AppendLine($"\n");
        }
        private void GenerateObjAssociation(JsonData.Model assoc)
        {


            sourceCodeBuilder.Append($"${assoc.name} = new Association(");
            
            foreach (var association in assoc.@class)
            {
                sourceCodeBuilder.Append($"\"{association.class_name}\",");
            }
  
            sourceCodeBuilder.Length -= 1; // Remove the last character (",")
   
            sourceCodeBuilder.AppendLine($");");
        }
        public class JsonData
        {
            public string type { get; set; }
            public string sub_id { get; set; }
            public string sub_name { get; set; }
            public List<Model> model { get; set; }
            public class Model  
            {
                public string type { get; set; }
                public string class_id { get; set; }
                public string class_name { get; set; }
                public string KL { get; set; }
                public string name { get; set; }
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
                public string state_id { get; set; }
                public string state_name { get; set; }
                public string state_value { get; set; }
                public string state_type { get; set; }
                public string[] state_event { get; set; }
                public string[] state_function { get; set; }
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
                richTextBox1.Text = displayJson;
                btn_copy1.Enabled = true;
                isTextCleared = false;
            }
            sourceCodeBuilder.Clear();
            
        }

        private void btn_translate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!isTextCleared && !string.IsNullOrEmpty(selectedJsonFilePath) && File.Exists(selectedJsonFilePath))
                {
                    sourceCodeBuilder.Clear();
                    GeneratePhpCode(selectedJsonFilePath);
                    label2.Text = string.Empty;
                    btn_copy2.Enabled = true;
                    btn_export.Enabled = true;
                }
                else if (isTextCleared)
                {
                    MessageBox.Show("Please load a new JSON file before translating.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btn_export_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PHP Files|*.php";
            saveFileDialog.Title = "Save PHP File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        // Menuliskan konten dari RichTextBox ke file PHP
                        sw.Write(richTextBox2.Text);
                    }

                    MessageBox.Show("File berhasil disimpan!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private bool isTextCleared = false;
        private void btn_clear_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
            btn_copy1.Enabled = false;
            btn_copy2.Enabled = false;
            btn_export.Enabled = false;

            isTextCleared = true;
        }

        private void btn_help_Click(object sender, EventArgs e)
        {
            MessageBox.Show("How to Use the Application:\n1. Upload your JSON file\n2. Click the 'Translate' button to generate the json into PHP\n3. The result will be displayed on the screen\n4. If you want to Export the result to a PHP file, please click the 'Save' button", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_copy1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(richTextBox1.Text))
            {
                Clipboard.SetText(richTextBox1.Text);
                MessageBox.Show("JSON content copied to clipboard!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please select JSON file first!", "Warning");
            }
        }

        private void btn_copy2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(richTextBox2.Text))
            {
                Clipboard.SetText(richTextBox2.Text);
                MessageBox.Show("PHP content copied to clipboard!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please translate JSON file first!", "Warning");
            }
        }
    }
}
