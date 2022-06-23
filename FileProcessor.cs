using System;
using System.Collections.Generic;
using Godot;
using ImageToPaletteGenerator;
using Newtonsoft.Json;

    public class FileProcessor : Reference
    {
        
        protected bool _useCustomFolderPath;
        protected String _folderPath;
        protected bool _useCustomSavePath;
        protected String _savePath;
        
        protected delegate void Process(string inputPath, string outputPath);
        protected int FilesProcessed;

        protected int TotalFiles;
        protected float Progress;
        
        protected void ProcessFolder( Process processFunction, String inputPath, string outputPath) {
            Directory dir = new Directory();
            List<string> files = new List<string>();
            if (dir.Open(inputPath) == Error.Ok)
            {
                files = GetFilePaths(dir, inputPath);
                TotalFiles = files.Count;
            }
            foreach (var file in files)
            {
                processFunction(file, outputPath);
            }
        }

        public static List<String> GetFilePaths(Directory dir, String folderPath)
        {
            List<string> files = new List<string>();
            dir.ListDirBegin();
            var fileName = dir.GetNext();
            while (fileName != String.Empty) {
                if (!dir.CurrentIsDir()) {
                    var path = folderPath + fileName;
                    files.Add(path);
                }
                fileName = dir.GetNext();
            }
            return files;
        }
        
        
        protected void WritePaletteToDisk(ColorPalette colorPalette, String name, String path) {
            var file = new File();
            var savePath = path + "colors_"+ name + ".json";
            file.Open(savePath, File.ModeFlags.Write);
            string output = JsonConvert.SerializeObject(colorPalette, Formatting.Indented);
            file.StoreLine(output);
            file.Close();
        }
    }
