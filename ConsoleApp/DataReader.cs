namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class DataReader
    {
        IEnumerable<ImportedObject> ImportedObjects;

        public DataReader()
        {
            ImportedObjects = new List<ImportedObject>();
        }

        public void ProcessImportAndPrintData(string fileToImport, bool printData = true)
        {
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            var fullPath = Path.Combine(projectDirectory, fileToImport);

            if (!File.Exists(fullPath))
            {
                return;
            }

            ReadFile(fullPath);

            CleanObjects();

            AssignNumber();

            PrintData();
        }

        /// <summary>
        /// Read data from file
        /// </summary>
        /// <param name="fileToImport"></param>
        private void ReadFile(string fileToImport)
        {
            var importedLines = new List<string>();

            try
            {
                using (var streamReader = new StreamReader(fileToImport))//!FileNotFoundException 
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        importedLines.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing the data: {ex.Message}");
            }

            for (int i = 0; i < importedLines.Count; i++) //If you use <= the loop may throw an exception if importedLines is empty.
            {
                var importedLine = importedLines[i];
                var values = importedLine.Split(';');
                try
                {
                    var importedObject = new ImportedObject
                    {
                        Type = values[0],
                        Name = values[1],
                        Schema = values[2],
                        ParentName = values[3],
                        ParentType = values[4],
                        DataType = values[5],
                        IsNullable = values[6]
                    };
                    ((List<ImportedObject>)ImportedObjects).Add(importedObject);
                }
                catch (IndexOutOfRangeException ex)
                {
                    Console.WriteLine($"{ex.Message},{importedLine}");
                    continue;
                }
            }
        }

        /// <summary>
        /// clear and correct imported data
        /// </summary>
        private void CleanObjects()
        {
            foreach (var importedObject in ImportedObjects)
            {
                if (string.IsNullOrWhiteSpace(importedObject.ToString()))
                {
                    continue;
                }
                //When converting strings to lowercase or uppercase, consider using ToLowerInvariant or ToUpperInvariant instead of ToLower or ToUpper,
                //as the former will give you consistent results across different cultures.
                importedObject.Type = importedObject.Type.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpperInvariant();
                importedObject.Name = importedObject.Name.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.Schema = importedObject.Schema.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentName = importedObject.ParentName.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentType = importedObject.ParentType.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
            }
        }

        /// <summary>
        /// assign number of children
        /// </summary>
        private void AssignNumber()
        {
            Dictionary<(string Type, string Name), int> childrenCount = new Dictionary<(string Type, string Name), int>();

            foreach (var impObj in ImportedObjects)
            {
                var key = (impObj.ParentType, impObj.ParentName);
                if (childrenCount.ContainsKey(key))
                {
                    childrenCount[key]++;
                }
                else
                {
                    childrenCount.Add(key, 1);
                }
            }

            foreach (var importedObject in ImportedObjects)
            {
                var key = (importedObject.Type, importedObject.Name);
                if (childrenCount.ContainsKey(key))
                {
                    importedObject.NumberOfChildren = childrenCount[key];
                }
            }
        }

        private void PrintData()
        {
            foreach (var database in ImportedObjects.Where(d => d.Type == "DATABASE"))
            {
                Console.WriteLine($"Database '{database.Name}' ({ImportedObjects.Count(t => t.ParentType == database.Type && t.ParentName == database.Name)} tables)");
                // print all database's tables
                foreach (var table in ImportedObjects.Where(t => t.ParentType == database.Type && t.ParentName == database.Name))
                {
                    Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({ImportedObjects.Count(c => c.ParentType == table.Type && c.ParentName == table.Name)} columns)");

                    // print all table's columns
                    foreach (var column in ImportedObjects.Where(c => c.ParentType == table.Type && c.ParentName == table.Name))
                    {
                        Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                    }
                }
            }

            Console.ReadLine();
        }


    }

    class ImportedObject : ImportedObjectBaseClass
    {
        //The ImportedObject class inherits from ImportedObjectBaseClass,
        ///but it re-declares a Name property, which is redundant 
        //and may lead to unexpected behavior.

        public string Schema;

        public string ParentName;
        public string ParentType
        {
            get; set;
        }

        public string DataType { get; set; }
        public string IsNullable { get; set; }

        public int NumberOfChildren; //Change to int

        public override string ToString()
        {
            return $"{Type}{Name}{Schema}{ParentName}{ParentType}";
        }
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
