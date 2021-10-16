﻿using Microsoft.Build.Framework;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AssemblyPublicizer
{
    public class PublicizeTask : Microsoft.Build.Utilities.Task
    {
        private const string OutputSuffix = "_public";

        [Required] public virtual ITaskItem[] InputAssemblies { get; set; } = { };

        [Required] public virtual string OutputDir { get; set; } = Directory.GetCurrentDirectory();

        public virtual bool PublicizeExplicitImpls { get; set; } = false;

        public virtual bool AsmPatcher { get; set; } = false;

        public override bool Execute()
        {
            Log.LogMessage($"Publicizing {InputAssemblies.Length} input assemblies provided");
            return InputAssemblies.Aggregate(true, (current, assembly) => current & PublicizeItem(assembly.ItemSpec));
        }

        private bool PublicizeItem (string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                Log.LogError($"Invalid path {assemblyPath}");
                return false;
            }
            var filename = Path.GetFileNameWithoutExtension(assemblyPath);
            var curHash = ComputeHash(assemblyPath);
            var hashPath = Path.Combine(OutputDir, $"{filename}{OutputSuffix}.hash");

            var lastHash = File.Exists(hashPath) ? File.ReadAllText(hashPath) : null;
            
            
            if (curHash == lastHash)
            {
                Log.LogMessage("Public assembly is up to date.");
                return true;
            }
            Log.LogMessage($"Generating publicized assembly from {assemblyPath}");
            
            PublicizableAssembly moduleDefinition = AsmPatcher ? new AsmModuleDef() : new DnModuleDef();
            moduleDefinition.Load(assemblyPath);
            moduleDefinition.Publicize(PublicizeExplicitImpls);
            var outputPath = Path.Combine(OutputDir, $"{filename}{OutputSuffix}.dll");
            moduleDefinition.Write(outputPath);
            return true;
        }

        private string ComputeHash(string filePath)
        {
            var res = new StringBuilder();

            using (var hash = SHA1.Create())
            {
                using (var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    hash.ComputeHash(file);
                    file.Close();
                }

                foreach (byte b in hash.Hash)
                    res.Append(b.ToString("X2"));
            }

            return res.ToString();
        }
    }
}