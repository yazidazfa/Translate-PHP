using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xtUML1
{
    class Parsing
    {
        public static bool Point1(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                HashSet<string> subsystemNames = new HashSet<string>();

                foreach (var item in jsonArray)
                {
                    if (item["type"]?.ToString() == "subsystem")
                    {
                        var subsystemProperty = item["sub_name"];

                        if (subsystemProperty != null)
                        {
                            string subsystemName = subsystemProperty.ToString();

                            if (subsystemNames.Contains(subsystemName))
                            {
                                textBox4.AppendText($"Syntax error 1: There is a subsystem with the same name: {subsystemName}. Ensure that all subsystems have unique names.\r\n");
                            }

                            subsystemNames.Add(subsystemName);
                        }
                        else
                        {
                            textBox4.AppendText("Syntax error 1: Property 'sub_name' not found or is empty.\r\n");
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 1: " + ex.Message + "\r\n");
                return false;
            }
        }
        public static bool Point2(Form1 form1, JArray jsonArray)
        {

            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                foreach (var subsystem in jsonArray)
                {
                    HashSet<string> classIdsInClass = new HashSet<string>();
                    HashSet<string> classIdsInAssociationClass = new HashSet<string>();

                    var modelArray = subsystem["model"] as JArray;

                    if (modelArray != null)
                    {
                        foreach (var item in modelArray)
                        {
                            var itemType = item["type"]?.ToString();

                            if (itemType == "class")
                            {
                                var classIdProperty = item["class_id"];
                                var attributesArray = item["attributes"] as JArray;

                                if (classIdProperty != null)
                                {
                                    string classId = classIdProperty.ToString();
                                    classIdsInClass.Add(classId);

                                    if (attributesArray == null || !attributesArray.Any())
                                    {
                                        textBox4.AppendText($"Syntax error 2: Class {classId} in subsystem {subsystem["sub_name"]?.ToString()} does not have attributes.\r\n");

                                    }
                                }
                            }
                            else if (itemType == "association")
                            {
                                var classArrayProperty = item["class"] as JArray;

                                if (classArrayProperty != null)
                                {
                                    foreach (var classItem in classArrayProperty)
                                    {
                                        var classIdProperty = classItem["class_id"];

                                        if (classIdProperty != null)
                                        {
                                            string classId = classIdProperty.ToString();
                                            classIdsInAssociationClass.Add(classId);
                                        }
                                    }
                                }

                                var associationClassModel = item["model"];
                                if (associationClassModel is JObject associationObject)
                                {
                                    var associationClassType = associationObject["type"]?.ToString();

                                    if (associationClassType == "association_class")
                                    {
                                        var classIdProperty = associationObject["class_id"];

                                        if (classIdProperty != null)
                                        {
                                            string classId = classIdProperty.ToString();
                                            classIdsInClass.Add(classId);
                                            classIdsInAssociationClass.Add(classId);

                                            var attributesArray = associationObject["attributes"] as JArray;
                                            if (attributesArray == null || !attributesArray.Any())
                                            {
                                                textBox4.AppendText($"Syntax error 2: Class {classId} in subsystem {subsystem["sub_name"]?.ToString()} does not have attributes.\r\n");

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var classesWithoutRelation = classIdsInClass.Except(classIdsInAssociationClass);

                    if (classesWithoutRelation.Any())
                    {
                        textBox4.AppendText($"Syntax error 2: There are classes without relationships in subsystem {subsystem["sub_name"]?.ToString()}. Class IDs without relationships: {string.Join(", ", classesWithoutRelation)}\r\n");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 2: " + ex.Message + "\r\n");
                return false;
            }
        }

        public static bool Point3(Form1 form1, JArray jsonArray)
        {

            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                Dictionary<string, HashSet<string>> classInfoMap = new Dictionary<string, HashSet<string>>();

                Func<JToken, bool> processItem = null;
                processItem = (item) =>
                {
                    var itemType = item["type"]?.ToString();

                    if (itemType == "class")
                    {
                        var className = item["class_name"]?.ToString();
                        var attributes = GetAttributesAsString(item["attributes"] as JArray);

                        var classInfo = $"{className}-{attributes}";

                        if (classInfoMap.ContainsKey(classInfo))
                        {
                            textBox4.AppendText($"Syntax error 3: Class {className} in subsystem {item["sub_name"]?.ToString()} has the same information as a class in another subsystem.\r\n");

                        }
                        else
                        {
                            classInfoMap.Add(classInfo, new HashSet<string>());
                        }
                    }
                    else if (itemType == "association")
                    {
                        var classArrayProperty = item["class"] as JArray;

                        if (classArrayProperty != null)
                        {
                            foreach (var classItem in classArrayProperty)
                            {
                                if (!processItem(classItem))
                                {
                                    return false;
                                }
                            }
                        }

                        var associationClassModel = item["model"];
                        if (associationClassModel is JObject associationObject)
                        {
                            if (!processItem(associationObject))
                            {
                                return false;
                            }
                        }
                    }
                    else if (itemType == "association_class")
                    {
                        var className = item["class_name"]?.ToString();
                        var attributes = GetAttributesAsString(item["attributes"] as JArray);

                        var classInfo = $"{className}-{attributes}";

                        if (classInfoMap.ContainsKey(classInfo))
                        {
                            textBox4.AppendText($"Syntax error 3: Class {className} in subsystem {item["sub_name"]?.ToString()} has identical information to a class in another subsystem.\r\n");

                        }
                        else
                        {
                            classInfoMap.Add(classInfo, new HashSet<string>());
                        }
                    }

                    return true;
                };

                foreach (var subsystem in jsonArray)
                {
                    foreach (var item in subsystem["model"])
                    {
                        if (!processItem(item))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error: " + ex.Message + "\r\n");
                return false;
            }
        }


        private static string GetAttributesAsString(JArray attributes)
        {
            if (attributes == null)
            {
                return string.Empty;
            }

            List<string> attributeStrings = new List<string>();

            foreach (var attribute in attributes)
            {
                var attributeType = attribute["attribute_type"]?.ToString();
                var attributeName = attribute["attribute_name"]?.ToString();
                var dataType = attribute["data_type"]?.ToString();

                attributeStrings.Add($"{attributeType}-{attributeName}-{dataType}");
            }

            return string.Join("|", attributeStrings);
        }

        public static bool Point4(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                foreach (var subsystem in jsonArray)
                {
                    HashSet<string> classNames = new HashSet<string>();
                    HashSet<string> classIds = new HashSet<string>();

                    foreach (var item in subsystem["model"])
                    {
                        var itemType = item["type"]?.ToString();

                        if ((itemType == "class" || itemType == "association_class") && item["class_name"] != null)
                        {
                            var className = item["class_name"]?.ToString();
                            var classId = item["class_id"]?.ToString();

                            if (string.IsNullOrWhiteSpace(className) || string.IsNullOrWhiteSpace(classId))
                            {
                                textBox4.AppendText("Syntax error 4: Class name or class_id is empty in the subsystem. \r\n");

                            }

                            if (classNames.Contains(className))
                            {
                                textBox4.AppendText($"Syntax error 4: Duplicate class name {className} within this subsystem. \r\n");

                            }

                            if (classIds.Contains(classId))
                            {
                                textBox4.AppendText($"Syntax error 4: Duplicate class_id {classId} within this subsystem. \r\n");

                            }

                            classNames.Add(className);
                            classIds.Add(classId);
                        }

                        if (itemType == "association" && item["model"] is JObject associationModel)
                        {
                            var associationItemType = associationModel["type"]?.ToString();

                            if (associationItemType == "association_class" && associationModel["class_name"] != null)
                            {
                                var associationClassName = associationModel["class_name"]?.ToString();
                                var associationClassId = associationModel["class_id"]?.ToString();

                                if (string.IsNullOrWhiteSpace(associationClassName) || string.IsNullOrWhiteSpace(associationClassId))
                                {
                                    textBox4.AppendText("Syntax error 4: Class name or class_id is empty in the subsystem. \r\n");

                                }

                                if (classNames.Contains(associationClassName))
                                {
                                    textBox4.AppendText($"Syntax error 4: Duplicate class name {associationClassName} within this subsystem. \r\n");

                                }

                                if (classIds.Contains(associationClassId))
                                {
                                    textBox4.AppendText($"Syntax error 4: Duplicate class_id {associationClassId} within this subsystem. \r\n");

                                }

                                classNames.Add(associationClassName);
                                classIds.Add(associationClassId);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 4: " + ex.Message + "\r\n");
                return false;
            }
        }

        public static bool Point5(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                foreach (var subsystem in jsonArray)
                {
                    HashSet<string> uniqueKLs = new HashSet<string>();
                    foreach (var item in subsystem["model"])
                    {
                        var itemType = item["type"]?.ToString();

                        if (itemType == "class" && item["class_name"] != null)
                        {
                            var className = item["class_name"]?.ToString();
                            var KL = item["KL"]?.ToString();

                            if (string.IsNullOrWhiteSpace(className) || string.IsNullOrWhiteSpace(KL))
                            {
                                textBox4.AppendText("Syntax error 5: Class name or KL is empty in the subsystem. \r\n");
                            }

                            if (uniqueKLs.Contains(KL))
                            {
                                textBox4.AppendText($"Syntax error 5: Duplicate KL value {KL} within this subsystem. \r\n");
                            }

                            uniqueKLs.Add(KL);
                        }

                        if (itemType == "association" && item["model"] is JObject associationModel)
                        {
                            var associationItemType = associationModel["type"]?.ToString();

                            if (associationItemType == "association_class" && associationModel["class_name"] != null)
                            {
                                var associationKL = associationModel["KL"]?.ToString();

                                if (string.IsNullOrWhiteSpace(associationKL))
                                {
                                    textBox4.AppendText("Syntax error 5: KL value is empty in the subsystem. \r\n");
                                }

                                if (uniqueKLs.Contains(associationKL))
                                {
                                    textBox4.AppendText($"Syntax error 5: Duplicate KL value {associationKL} within this subsystem. \r\n");

                                }

                                uniqueKLs.Add(associationKL);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 5: " + ex.Message + "\r\n");
                return false;
            }
        }

        public static bool Point6(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                foreach (var subsystem in jsonArray)
                {

                    foreach (var item in subsystem["model"])
                    {
                        var itemType = item["type"]?.ToString();

                        if (itemType == "class" && item["class_name"] != null)
                        {
                            var className = item["class_name"]?.ToString();
                            var attributes = item["attributes"] as JArray;

                            if (attributes != null)
                            {
                                HashSet<string> uniqueAttributeNames = new HashSet<string>();

                                foreach (var attribute in attributes)
                                {
                                    var attributeName = attribute["attribute_name"]?.ToString();

                                    if (string.IsNullOrWhiteSpace(attributeName))
                                    {
                                        textBox4.AppendText($"Syntax error 6: Attribute name is empty in class {className}. \r\n");

                                    }

                                    if (uniqueAttributeNames.Contains(attributeName))
                                    {
                                        textBox4.AppendText($"Syntax error 6: Duplicate attribute name {attributeName} in class {className}. \r\n");

                                    }

                                    uniqueAttributeNames.Add(attributeName);
                                }
                            }
                        }

                        if (itemType == "association" && item["model"] is JObject associationModel)
                        {
                            var associationItemType = associationModel["type"]?.ToString();

                            if (associationItemType == "association_class" && associationModel["class_name"] != null)
                            {
                                var associationClassName = associationModel["class_name"]?.ToString();
                                var associationAttributes = associationModel["attributes"] as JArray;

                                if (associationAttributes != null)
                                {
                                    HashSet<string> uniqueAssociationAttributeNames = new HashSet<string>();

                                    foreach (var attribute in associationAttributes)
                                    {
                                        var attributeName = attribute["attribute_name"]?.ToString();

                                        if (string.IsNullOrWhiteSpace(attributeName))
                                        {
                                            textBox4.AppendText($"Syntax error 6: Attribute name is empty in association class {associationClassName}. \r\n");

                                        }

                                        if (uniqueAssociationAttributeNames.Contains(attributeName))
                                        {
                                            textBox4.AppendText($"Syntax error 6: Duplicate attribute name {attributeName} in association class {associationClassName}. \r\n");

                                        }

                                        uniqueAssociationAttributeNames.Add(attributeName);
                                    }
                                }
                            }
                        }

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 6: " + ex.Message + "\r\n");
                return false;
            }
        }

        public static bool Point7(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                foreach (var subsystem in jsonArray)
                {
                    foreach (var item in subsystem["model"])
                    {
                        var itemType = item["type"]?.ToString();

                        if (itemType == "class" && item["class_name"] != null)
                        {
                            var className = item["class_name"]?.ToString();
                            var attributes = item["attributes"] as JArray;

                            if (attributes != null)
                            {
                                bool hasPrimaryKey = false;

                                foreach (var attribute in attributes)
                                {
                                    var attributeType = attribute["attribute_type"]?.ToString();

                                    if (attributeType == "naming_attribute")
                                    {
                                        hasPrimaryKey = true;
                                        break;
                                    }
                                }

                                if (!hasPrimaryKey)
                                {
                                    textBox4.AppendText($"Syntax error 7: Class {className} does not have a primary key. \r\n");

                                }
                            }
                            else
                            {
                                textBox4.AppendText($"Syntax error 7: Class {className} does not have any attributes. \r\n");

                            }
                        }

                        if (itemType == "association" && item["model"] is JObject associationModel)
                        {
                            var associationItemType = associationModel["type"]?.ToString();

                            if (associationItemType == "association_class" && associationModel["class_name"] != null)
                            {
                                var associationClassName = associationModel["class_name"]?.ToString();
                                var associationAttributes = associationModel["attributes"] as JArray;

                                if (associationAttributes != null)
                                {
                                    bool hasPrimaryKey = false;

                                    foreach (var attribute in associationAttributes)
                                    {
                                        var attributeType = attribute["attribute_type"]?.ToString();

                                        if (attributeType == "naming_attribute")
                                        {
                                            hasPrimaryKey = true;
                                            break;
                                        }
                                    }

                                    if (!hasPrimaryKey)
                                    {
                                        textBox4.AppendText($"Syntax error 7: Association Class {associationClassName} does not have a primary key. \r\n");

                                    }
                                }
                                else
                                {
                                    textBox4.AppendText($"Syntax error 7: Association Class {associationClassName} does not have any attributes. \r\n");

                                }
                            }
                        }

                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 7: " + ex.Message + "\r\n");
                return false;
            }
        }

        public static bool Point8(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                foreach (var subsystem in jsonArray)
                {
                    HashSet<string> associationNames = new HashSet<string>();

                    foreach (var item in subsystem["model"])
                    {
                        var itemType = item["type"]?.ToString();

                        if (itemType == "association" && item["name"] != null)
                        {
                            var associationName = item["name"]?.ToString();

                            if (string.IsNullOrWhiteSpace(associationName))
                            {
                                textBox4.AppendText("Syntax error 8: Association name is empty in the subsystem. \r\n");

                            }

                            if (associationNames.Contains(associationName))
                            {
                                textBox4.AppendText($"Syntax error 8: Duplicate association name {associationName} within this subsystem.\r\n");

                            }

                            associationNames.Add(associationName);
                        }
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 8: " + ex.Message + "\r\n");
                return false;
            }
        }

        public static bool Point9(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                foreach (var subsystem in jsonArray)
                {
                    foreach (var item in subsystem["model"])
                    {
                        var itemType = item["type"]?.ToString();

                        if (itemType == "association")
                        {
                            var class1Multiplicity = item["class"][0]["class_multiplicity"]?.ToString();
                            var class2Multiplicity = item["class"][1]["class_multiplicity"]?.ToString();

                            if ((class1Multiplicity == "0..*" && class2Multiplicity == "0..*") || (class1Multiplicity == "0..*" && class2Multiplicity == "1..*") || (class1Multiplicity == "1..*" && class2Multiplicity == "0..*") || (class1Multiplicity == "1..*" && class2Multiplicity == "1..*"))
                            {
                                var associationModel = item["model"];
                                if (associationModel == null)
                                {
                                    textBox4.AppendText($"Syntax error 9: Relationship {item["name"]?.ToString()} (many-to-many) has not been formalized with an association_class. \r\n");
                                }

                                if (associationModel != null && associationModel["type"]?.ToString() != "association_class")
                                {
                                    textBox4.AppendText($"Syntax error 9: Relationship {item["name"]?.ToString()} (many-to-many) has not been formalized with an association_class. \r\n");
                                }
                            }
                            else if ((class1Multiplicity == "0..*" && class2Multiplicity == "1..1") ||
                                     (class1Multiplicity == "1..1" && class2Multiplicity == "0..*") ||
                                     (class1Multiplicity == "1..*" && class2Multiplicity == "1..1") ||
                                     (class1Multiplicity == "1..1" && class2Multiplicity == "1..*") ||
                                     (class1Multiplicity == "1..1" && class2Multiplicity == "1..1"))
                            {
                                var class1Id = item["class"][0]["class_id"]?.ToString();
                                var class2Id = item["class"][1]["class_id"]?.ToString();

                                if (!HasReferentialAttribute(jsonArray, class1Id) && !HasReferentialAttribute(jsonArray, class2Id))
                                {
                                    textBox4.AppendText($"Syntax error 9: One of the Class {class1Id} or {class2Id} in relationship {item["name"]?.ToString()} (one-to-one) must be formalized with a referential_attribute. \r\n");

                                }
                            }
                        }
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 9: " + ex.Message + "\r\n");
                return false;
            }
        }
        public static bool HasReferentialAttribute(JArray jsonArray, string classId)
        {
            foreach (var subsystem in jsonArray)

            {
                foreach (var item in subsystem["model"])
                {
                    var itemType = item["type"]?.ToString();

                    if (itemType == "class")
                    {
                        var currentClassId = item["class_id"]?.ToString();

                        if (currentClassId == classId)
                        {
                            var attributes = item["attributes"] as JArray;

                            if (attributes != null)
                            {
                                foreach (var attribute in attributes)
                                {
                                    var attributeType = attribute["attribute_type"]?.ToString();

                                    if (attributeType == "referential_attribute")
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static bool Point10(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();

            try
            {
                // Check for null values and empty array
                if (jsonArray == null || jsonArray.Count == 0)
                {
                    textBox4.AppendText("Syntax error 10: JSON array is null or empty.\r\n");
                    return false;
                }

                // Mengumpulkan semua nama referential attribute dari type class dan type association_class
                HashSet<string> referentialAttributeNames = new HashSet<string>();

                foreach (var subsystem in jsonArray)
                {
                    // Check for null values
                    if (subsystem == null || subsystem["model"] == null)
                        continue;

                    foreach (var item in subsystem["model"])
                    {
                        // Check for null values
                        if (item == null || item["type"] == null || item["attributes"] == null)
                            continue;

                        if (item["type"].ToString() == "class")
                        {
                            JArray classAttributes = (JArray)item["attributes"];
                            foreach (JObject attribute in classAttributes)
                            {
                                // Check for null values
                                if (attribute == null || attribute["attribute_type"] == null || attribute["attribute_name"] == null)
                                    continue;

                                if (attribute["attribute_type"].ToString() == "referential_attribute")
                                {
                                    string attributeName = attribute["attribute_name"].ToString();
                                    referentialAttributeNames.Add(attributeName);
                                }
                            }
                        }

                        if (item["type"].ToString() == "association" && item["model"] is JObject associationModel)
                        {
                            // Check for null values
                            if (associationModel["type"] == null || associationModel["class_name"] == null || associationModel["attributes"] == null)
                                continue;

                            var associationItemType = associationModel["type"].ToString();

                            if (associationItemType == "association_class")
                            {
                                JArray associationClassAttributes = (JArray)associationModel["attributes"];

                                foreach (JObject attribute in associationClassAttributes)
                                {
                                    // Check for null values
                                    if (attribute == null || attribute["attribute_type"] == null || attribute["attribute_name"] == null)
                                        continue;

                                    if (attribute["attribute_type"].ToString() == "referential_attribute")
                                    {
                                        string attributeName = attribute["attribute_name"].ToString();
                                        referentialAttributeNames.Add(attributeName);
                                    }
                                }
                            }
                        }
                    }
                }

                // Iterasi setiap referential attribute dan periksa penamaannya
                foreach (string attributeName in referentialAttributeNames)
                {
                    // Ambil bagian sebelum _id
                    string[] parts = attributeName.Split('_');

                    // Check for null values
                    if (parts == null || parts.Length < 2)
                    {
                        textBox4.AppendText($"Syntax error 10: Referential attribute '{attributeName}' has incorrect naming.\r\n");
                        return false;
                    }

                    string referenceName = string.Join("_", parts.Take(parts.Length - 1));
                    string lastPart = parts.LastOrDefault(); // Ambil bagian terakhir

                    // Periksa apakah ada kelas dengan KL yang mengandung referenceName
                    bool isValid = IsReferenceNameValid(jsonArray, referenceName);

                    if (!isValid || (lastPart != "id"))
                    {
                        textBox4.AppendText($"Syntax error 10: Referential attribute '{attributeName}' has incorrect naming.\r\n");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 10: " + ex.Message + "\r\n");
                return false;
            }
        }

        // ...



        public static bool IsReferenceNameValid(JArray jsonArray, string referenceName)
        {
            // Iterasi setiap kelas dan association_class dan periksa KL-nya
            foreach (var subsystem in jsonArray)
            {
                foreach (var item in subsystem["model"])
                {
                    if (item["type"].ToString() == "class")
                    {
                        string klValue = item["KL"]?.ToString();
                        if (!string.IsNullOrEmpty(klValue) && klValue.Contains(referenceName))
                        {
                            return true;
                        }
                    }

                    if (item["type"].ToString() == "association" && item["model"] is JObject associationModel)
                    {
                        var associationItemType = associationModel["type"]?.ToString();

                        if (associationItemType == "association_class")
                        {
                            string klValue = associationModel["KL"]?.ToString();
                            if (!string.IsNullOrEmpty(klValue) && klValue.Contains(referenceName))
                            {
                                return true;
                            }
                        }
                    }


                }
            }

            return false;
        }
        public static bool Point11(Form1 form1, JArray subsystems)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                foreach (var subsystem in subsystems)
                {
                    var subsystemId = subsystem["sub_id"]?.ToString();

                    foreach (var item in subsystem["model"])
                    {
                        var itemType = item["type"]?.ToString();

                        if (itemType == "association")
                        {
                            var class1Id = item["class"][0]["class_id"]?.ToString();
                            var class2Id = item["class"][1]["class_id"]?.ToString();

                            if (!IsClassInSubsystem(subsystem, class1Id))
                            {
                                if (!TryFindImportedClassInOtherSubsystem(subsystem, class1Id))
                                {
                                    textBox4.AppendText($"Syntax error 11: Subsystem {subsystemId} does not have a corresponding class or imported class for relationship {item["name"]?.ToString()}. \r\n");

                                }
                            }

                            if (!IsClassInSubsystem(subsystem, class2Id))
                            {
                                if (!TryFindImportedClassInOtherSubsystem(subsystem, class2Id))
                                {
                                    textBox4.AppendText($"Syntax error 11: Subsystem {subsystemId} does not have a class or imported class corresponding to the relationship {item["name"]?.ToString()}. \r\n");

                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 11: " + ex.Message + "\r\n");
                return false;
            }
        }

        private static bool IsClassInSubsystem(JToken subsystem, string classId)
        {
            foreach (var item in subsystem["model"])
            {
                var itemType = item["type"]?.ToString();

                if (itemType == "class")
                {
                    var currentClassId = item["class_id"]?.ToString();

                    if (currentClassId == classId)
                    {
                        return true;
                    }
                }

                if (itemType == "association" && item["model"] is JObject associationModel)
                {
                    var associationItemType = associationModel["type"]?.ToString();

                    if (associationItemType == "association_class")
                    {
                        var classIdCurrent = associationModel["class_id"]?.ToString();

                        if (classIdCurrent == classId)
                        {
                            return true;
                        }

                    }
                }
            }

            return false;
        }

        private static bool TryFindImportedClassInOtherSubsystem(JToken subsystem, string classId)
        {
            foreach (var item in subsystem["model"])
            {
                var itemType = item["type"]?.ToString();

                if (itemType == "imported_class")
                {
                    var currentClassId = item["class_id"]?.ToString();

                    if (currentClassId == classId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool Point13(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();

            try
            {
                foreach (var subsistem in jsonArray)
                {
                    if (subsistem?["type"]?.ToString() == "subsystem")
                    {
                        var model = subsistem["model"];

                        foreach (var item in model)
                        {
                            if (item?["type"]?.ToString() == "class")
                            {
                                if (!CekKelas(item))
                                {
                                    textBox4.AppendText($"Syntax error 13: Class or class attribute {item["class_name"]} is incomplete. \r\n");
                                }
                            }
                            else if (item?["type"]?.ToString() == "association")
                            {
                                if (!CekRelasi(item))
                                {
                                    textBox4.AppendText($"Syntax error 13: Association class or relationship {item["name"]} is incomplete. \r\n");
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 13: " + ex.Message + "\r\n");
                return false;
            }
        }

        private static bool CekKelas(JToken kelas)
        {
            if (kelas?["class_name"] == null || kelas?["class_id"] == null || kelas?["KL"] == null)
            {
                return false;
            }

            var attributes = kelas?["attributes"];

            if (attributes != null)
            {
                var attributeNames = new HashSet<string>();

                foreach (var attribute in attributes)
                {
                    if (attribute?["attribute_type"] != null)
                    {
                        if (attribute?["attribute_name"] == null || attribute?["data_type"] == null)
                        {
                            return false;

                        }

                        if (attributeNames.Contains(attribute["attribute_name"].ToString()))
                        {
                            return false;
                        }

                        attributeNames.Add(attribute["attribute_name"].ToString());
                    }
                }
            }

            return true;
        }

        private static bool CekRelasi(JToken relasi)
        {
            if (relasi?["name"] == null || relasi?["class"] == null)
            {
                return false;
            }

            var classes = relasi?["class"];

            foreach (var kelas in classes)
            {
                if (kelas?["class_multiplicity"] == null)
                {
                    return false;
                }
            }

            if (relasi?["model"] != null && relasi?["model"]["type"]?.ToString() == "association_class")
            {
                if (!CekKelas(relasi?["model"]))
                {
                    return false;
                }
            }

            return true;
        }


        public static bool Point14(Form1 form1, JArray subsystems)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                foreach (var currentSubsystem in subsystems)
                {
                    var currentSubId = currentSubsystem["sub_id"]?.ToString();

                    foreach (var item in currentSubsystem["model"])
                    {
                        var itemType = item["type"]?.ToString();

                        if (itemType == "association")
                        {
                            var class1Id = item["class"][0]["class_id"]?.ToString();
                            var class2Id = item["class"][1]["class_id"]?.ToString();

                            if (!IsClassInSubsystem(currentSubsystem, class1Id))
                            {
                                if (!TryFindImportedClassInOtherSubsystem(currentSubsystem, class1Id))
                                {
                                    textBox4.AppendText($"Syntax error 14: Subsystem {currentSubsystem["sub_name"]} does not have a corresponding class or imported class for the relationship {item["name"]?.ToString()}.\r\n");

                                }

                                if (!IsRelationshipInOtherSubsystem(subsystems, currentSubsystem, class1Id, class2Id))
                                {
                                    textBox4.AppendText($"Syntax error 14: Subsystem {currentSubId} has a relationship with class_id {class1Id} or {class2Id}, but there is no corresponding relationship in other subsystems. \r\n");

                                }
                            }

                            if (!IsClassInSubsystem(currentSubsystem, class2Id))
                            {
                                if (!TryFindImportedClassInOtherSubsystem(currentSubsystem, class2Id))
                                {
                                    textBox4.AppendText($"Syntax error 14: Subsystem {currentSubsystem["sub_name"]} does not have a corresponding class or imported class for the relationship {item["name"]?.ToString()}. \r\n");

                                }

                                if (!IsRelationshipInOtherSubsystem(subsystems, currentSubsystem, class1Id, class2Id))
                                {
                                    textBox4.AppendText($"Syntax error 14: Subsystem {currentSubId} has a relationship with class_id {class1Id} or {class2Id}, but there is no corresponding relationship in other subsystems. \r\n");

                                }
                            }

                        }
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 14: " + ex.Message + "\r\n");
                return false;
            }
        }

        private static bool IsRelationshipInOtherSubsystem(JArray subsystems, JToken currentSubsystem, string class1Id, string class2Id)
        {
            var currentSubId = currentSubsystem["sub_id"]?.ToString();

            foreach (var otherSubsystem in subsystems)
            {
                if (otherSubsystem != currentSubsystem)
                {
                    foreach (var item in otherSubsystem["model"])
                    {
                        var itemType = item["type"]?.ToString();

                        if (itemType == "association" && item != null)
                        {
                            var otherClass1Id = item["class"][0]["class_id"]?.ToString();
                            var otherClass2Id = item["class"][1]["class_id"]?.ToString();

                            // Cek apakah relasi dengan class1Id dan class2Id ditemukan di subsistem lain
                            if ((otherClass1Id == class1Id && otherClass2Id == class2Id) ||
                                (otherClass2Id == class1Id && otherClass1Id == class2Id))
                            {
                                return true;
                            }
                        }
                    }

                }
            }

            return false;
        }


        public static bool Point15(Form1 form1, JArray subsystems)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                foreach (var subsystem in subsystems)
                {
                    foreach (var item in subsystem["model"])
                    {
                        var itemType = item["type"]?.ToString();

                        if (itemType == "class" && item["states"] is JArray states)
                        {
                            var className = item["class_name"]?.ToString();

                            foreach (var state in states)
                            {
                                var stateName = state["state_name"]?.ToString();

                                // Cek apakah ada state_name yang sama dengan class_name
                                if (stateName != null && stateName.Equals(className, StringComparison.OrdinalIgnoreCase))
                                {

                                    return true;
                                }
                            }

                            textBox4.AppendText($"Syntax error 15: Subsystem {subsystem["sub_id"]?.ToString()} has a class {className} without a corresponding state. \r\n");

                        }
                    }
                }

                return true;
            }

            catch (Exception ex)
            {
                textBox4.AppendText($"Syntax error 15: {ex.Message} \r\n");
                return false;
            }
        }

        public static bool Point25(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                Func<JToken, bool> processItem = null;
                processItem = (item) =>
                {
                    var itemType = item["type"]?.ToString();

                    if (itemType == "class" && item["class_name"]?.ToString() == "mahasiswa")
                    {
                        var className = item["class_name"]?.ToString();
                        var statesArray = item["states"] as JArray;

                        if (statesArray != null)
                        {
                            HashSet<string> stateNameInfo = new HashSet<string>();
                            foreach (var state in statesArray)
                            {
                                var stateName = state["state_name"]?.ToString();
                                if (!stateNameInfo.Add(stateName))
                                {
                                    textBox4.AppendText($"Syntax error 25: {stateName} state in {className} class contains the same information with other state. \r\n");
                                    return false;
                                }
                                if (stateName == null)
                                {
                                    textBox4.AppendText($"Syntax error 25: state name memiliki nilai null pada class {className}. \r\n");
                                    return false;
                                }
                            }
                        }
                        if (statesArray != null)
                        {
                            HashSet<string> stateValueInfo = new HashSet<string>();
                            foreach (var state in statesArray)
                            {
                                var stateValue = state["state_value"]?.ToString();
                                if (!stateValueInfo.Add(stateValue))
                                {
                                    textBox4.AppendText($"Syntax error 25: state dengan value {stateValue} pada class {className} memiliki informasi yang sama dengan state lain. \r\n");
                                    return false;
                                }
                            }
                        }
                    }
                    if (itemType == "class" && item["class_name"]?.ToString() == "dosen")
                    {
                        var className = item["class_name"]?.ToString();
                        var statesArray = item["states"] as JArray;

                        if (statesArray != null)
                        {
                            HashSet<string> stateNameInfo = new HashSet<string>();
                            foreach (var state in statesArray)
                            {
                                var stateName = state["state_name"]?.ToString();
                                if (!stateNameInfo.Add(stateName))
                                {
                                    textBox4.AppendText($"Syntax error 25: state dengan nama {stateName} pada class {className} memiliki informasi yang sama dengan state lain. \r\n");
                                    return false;
                                }
                                if (stateName == null)
                                {
                                    textBox4.AppendText($"Syntax error 25: state name memiliki nilai null pada class {className}. \r\n");
                                    return false;
                                }
                            }
                        }
                        if (statesArray != null)
                        {
                            HashSet<string> stateValueInfo = new HashSet<string>();
                            foreach (var state in statesArray)
                            {
                                var stateValue = state["state_value"]?.ToString();
                                if (!stateValueInfo.Add(stateValue))
                                {
                                    textBox4.AppendText($"Syntax error 25: state dengan value {stateValue} pada class {className} memiliki informasi yang sama dengan state lain. \r\n");
                                    return false;
                                }
                            }
                        }
                        if (statesArray != null)
                        {
                            HashSet<string> stateEvenInfo = new HashSet<string>();
                            foreach (var state in statesArray)
                            {
                                var stateEvent = state["state_event"]?.ToString();
                                if (!stateEvenInfo.Add(stateEvent))
                                {
                                    textBox4.AppendText($"Syntax error 25: state dengan event {stateEvent} pada class {className} meiliki informasi yang sama dengan state lain. \r\n");
                                    return false;
                                }
                            }
                        }
                        if (statesArray != null)
                        {
                            HashSet<string> stateEvenInfo = new HashSet<string>();
                            foreach (var state in statesArray)
                            {
                                var stateEvent = state["state_event"]?.ToString();
                                if (!stateEvenInfo.Add(stateEvent))
                                {
                                    textBox4.AppendText($"Syntax error 25: state dengan event {stateEvent} pada class {className} meiliki informasi yang sama dengan state lain. \r\n");
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                };

                foreach (var subsystem in jsonArray)
                {
                    foreach (var item in subsystem["model"])
                    {
                        if (!processItem(item))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 25: " + ex.Message + ".\r\n");
                return false;
            }
        }

        public static bool Point27(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();

            try
            {
                HashSet<string> strings = new HashSet<string>();
                Func<JToken, bool> processItem = null;
                processItem = (item) =>
                {
                    var itemType = item["type"]?.ToString();
                    if (itemType == "class")
                    {
                        var className = item["class_name"]?.ToString();
                        if (className == "mahasiswa" || className == "dosen")
                        {
                            var stateArray = item["states"] as JArray;
                            if (stateArray != null)
                            {
                                foreach (var state in stateArray)
                                {
                                    var stateName = state["state_name"]?.ToString();
                                    var stateEvent = state["state_event"]?.ToString();
                                    if (stateEvent != null)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        textBox4.AppendText($"Syntax error 27: event for {stateName} state is not implemented. \r\n");
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                textBox4.AppendText($"Syntax error 27: states label for class {className} is not implemented. \r\n");
                                return false;
                            }
                        }
                    }
                    return true;
                };
                foreach (var subsystem in jsonArray)
                {
                    foreach (var item in subsystem["model"])
                    {
                        if (!processItem(item))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 27: " + ex.Message + ".\r\n");
                return false;
            }
        }

        public static bool Point28(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                Func<JToken, bool> processItem = null;
                processItem = (item) =>
                {
                    var itemType = item["type"]?.ToString();
                    if (itemType == "class")
                    {
                        var className = item["class_name"]?.ToString();
                        var classId = item["class_id"]?.ToString();
                        var classKL = item["KL"]?.ToString();
                        if (className != null && classId != null && classKL != null)
                        {
                            return true;
                        }
                    }
                    return true;
                };
                foreach (var subsystem in jsonArray)
                {
                    foreach (var item in subsystem["model"])
                    {
                        if (!processItem(item))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 28: " + ex.Message + ".\r\n");
                return false;
            }
        }


        public static bool Point29(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                Func<JToken, bool> processItem = null;
                processItem = (item) =>
                {
                    var itemType = item["type"]?.ToString();
                    if (itemType == "class")
                    {
                        var className = item["class_name"]?.ToString();
                        if (className == "mahasiswa")
                        {
                            HashSet<string> idState = new HashSet<string>();
                            HashSet<string> stateN = new HashSet<string>();
                            var states = item["states"] as JArray;
                            if (states != null)
                            {
                                foreach (var sub in states)
                                {
                                    var stateName = sub["state_name"]?.ToString();
                                    var stateId = sub["state_id"]?.ToString();
                                    var stateValue = sub["state_value"]?.ToString();
                                    if (stateId != null)
                                    {
                                        idState.Add(stateId);
                                    }
                                    else if (stateName != null)
                                    {
                                        stateN.Add(stateName);
                                    }
                                    var transitions = sub["transitions"] as JArray;
                                    if (transitions != null)
                                    {
                                        foreach (var subTrans in transitions)
                                        {
                                            var stateTarget = subTrans["target_state"]?.ToString();
                                            var idStateTarget = subTrans["target_state_id"]?.ToString();
                                            if (stateTarget != null && idStateTarget != null)
                                            {
                                                idState.Add(idStateTarget);
                                                stateN.Add(stateTarget);
                                                if (!idState.Add(idStateTarget))
                                                {
                                                    return true;
                                                }
                                                else if (!stateN.Add(stateTarget))
                                                {
                                                    return true;
                                                }
                                                else
                                                {
                                                    textBox4.AppendText($"Syntax error 29: transition {className} class include incorect indentifier for target state {stateTarget}.\r\n");
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                textBox4.AppendText($"Syntax error 29: target id for transition class is null in class {className}.\r\n");
                                                return false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        textBox4.AppendText($"Syntax error 29: Transition state for class {className} is not implemented.\r\n");
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                textBox4.AppendText($"Syntax error 29: States for class {className} is null.\r\n");
                                return false;
                            }
                        }
                    }
                    return true;
                };
                foreach (var subsystem in jsonArray)
                {
                    foreach (var item in subsystem["model"])
                    {
                        if (!processItem(item))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 29: " + ex.Message + ".\r\n");
                return false;
            }
        }


        public static bool Point30(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();

            try
            {
                Func<JToken, bool> processItem = null;
                processItem = (item) =>
                {
                    var itemType = item["type"]?.ToString();
                    if (itemType == "class")
                    {
                        var className = item["class_name"]?.ToString();
                        var stateArray = item["states"] as JArray;
                        if (stateArray != null)
                        {
                            foreach (var state in stateArray)
                            {
                                HashSet<string> strings = new HashSet<string>();
                                var stateSetTimer = state["state_event"]?.ToString();
                                var stateName = state["state_name"]?.ToString();
                                if (stateSetTimer != null && !stateSetTimer.Contains("setTimer"))
                                {
                                    textBox4.AppendText("Syntax error 30: Every state in classes has not been included in the timer setting.\r\n");
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                            foreach (var state in stateArray)
                            {
                                HashSet<string> strings = new HashSet<string>();
                                var stateName = state["state_name"]?.ToString();
                                var stateSetTimer = state["state_event"]?.ToString();
                                if (stateSetTimer != null && !stateSetTimer.Contains("set"))
                                {
                                    textBox4.AppendText("Syntax error 30: Every state in classes has not been included in the timer setting.\r\n");
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return true;
                };
                foreach (var subsystem in jsonArray)
                {
                    foreach (var item in subsystem["model"])
                    {
                        if (!processItem(item))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText("Syntax error 30: " + ex.Message + ".\r\n");
                return false;
            }
        }

        public static bool Point34(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                Func<JToken, bool> processItem = null;
                processItem = (item) =>
                {
                    var itemType = item["type"]?.ToString();
                    if (itemType == "class")
                    {
                        var className = item["class_name"]?.ToString();
                        var states = item["states"] as JArray;
                        if (states != null)
                        {
                            foreach (var sub in states)
                            {
                                var stateName = sub["state_name"]?.ToString();
                                var stateEvent = sub["state_event"]?.ToString();
                                if (stateEvent != null)
                                {
                                    return true;
                                }
                                else
                                {
                                    textBox4.AppendText($"Syntax error 34: there is not action for {className} class itself.\r\n");
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            textBox4.AppendText($"Syntax error 34: states for {className} class is null.\r\n");
                            return false;
                        }
                    }
                    return true;
                };
                foreach (var subsystem in jsonArray)
                {
                    foreach (var item in subsystem["model"])
                    {
                        if (item.Contains("states"))
                        {
                            if (!processItem(item))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText($"Syntax error 34: " + ex.Message + ".\r\n");
                return false;
            }
        }

        public static bool Point35(Form1 form1, JArray jsonArray)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                HashSet<string> change = new HashSet<string>();
                Func<JToken, bool> processItem = null;
                processItem = (item) =>
                {
                    List<string> atributesList = new List<string>();
                    var itemType = item["type"]?.ToString();
                    if (itemType == "class")
                    {
                        var className = item["class_name"]?.ToString();
                        var classId = item["class_id"]?.ToString();
                        var classKl = item["KL"]?.ToString();
                        var atributes = item["atributes"] as JArray;
                        if (atributes != null)
                        {
                            atributesList.Add(atributes.ToString());
                            return true;
                        }
                        else
                        {
                            textBox4.AppendText($"Syntax error 35: atributes for {className} class is null.\r\n");
                            return false;
                        }
                    }
                    else if (itemType == "class_change")
                    {
                        var atributes = item["atributes"] as JArray;
                    }
                    else
                    {
                        textBox4.AppendText($"Syntax error 35: action for changing class description is not implemented yet.\r\n");
                        return false;
                    }
                    return true;
                };
                foreach (var subsystem in jsonArray)
                {
                    foreach (var item in subsystem["model"])
                    {
                        if (!processItem(item))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText($"Syntax error 35: " + ex.Message + ".\r\n");
                return false;
            }
        }

        public static bool Point99(Form1 form1, JArray subsystems)
        {
            RichTextBox textBox4 = form1.GetMessageBox();
            try
            {
                foreach (var subsystem in subsystems)
                {
                    var subsystemId = subsystem["sub_id"]?.ToString();

                    foreach (var item in subsystem["model"])
                    {
                        var itemType = item["type"]?.ToString();

                        if (itemType == "association")
                        {
                            var associationClass = item["class"] as JArray;

                            // Pastikan association memiliki dua class di dalamnya
                            if (associationClass == null || associationClass.Count != 2)
                            {
                                textBox4.AppendText($"Syntax error 99: Subsystem {subsystemId} has an association {item["name"]?.ToString()} that lacks a relationship between two classes within it. \r\n");

                            }
                        }
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                textBox4.AppendText($"Syntax error 99: {ex.Message} \r\n");
                return false;
            }
        }
    }
}
