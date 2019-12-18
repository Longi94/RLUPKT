using RLUPKT.Core;
using RLUPKT.Core.Encryption;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;

namespace RLUPKT.ConsoleApp
{
    internal class Program
    {
        class Options
        {
            [Option('t', "tfc", Default = false,
                HelpText = "Copy the tfc files from the input folder to the output folder.")]
            public bool CopyTfc { get; set; }

            [Option('i', "input", Required = true, HelpText = "Input folder containing the original upk packages.")]
            public string Input { get; set; }

            [Option('o', "output", Required = true,
                HelpText = "Output folder where the decrypted upk packages will be placed.")]
            public string Output { get; set; }
        }

        private static void ProcessFile(string filePath, string outputPath)
        {
            using (var output = File.Open(outputPath, FileMode.Create))
            {
                var upkFile = new UPKFile(filePath);
                var decryptionState = upkFile.Decrypt(output);
                if (decryptionState == DecryptionState.Failed)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    Console.WriteLine($"{fileName}: Unable to decrypt. possibly wrong AES-key");
                    output.Close();
                    File.Delete(outputPath);
                    //throw new InvalidDataException("Did not mange to decrypt the package");
                }
                //for (int i = 0; i < AESKeys.KeyList.Count; i++)
                //{
                //    try
                //    {
                //        var key = AESKeys.KeyList[i];
                //        upkFile.Decrypt(new RLDecryptor().GetCryptoTransform(key), output);
                //        AESKeys.KeyListSuccessCount[i] += 1;
                //        break;
                //    }
                //    catch (Exception e)
                //    {
                //        if (i + 1 != AESKeys.KeyList.Count)
                //        {
                //            continue;
                //        }else
                //        {
                //            string fileName = Path.GetFileNameWithoutExtension(filePath);
                //            Console.WriteLine($"{fileName}: Unable to decrypt. possibly wrong AES-key");
                //            output.Close();
                //            File.Delete(outputPath);
                //        }

                //    }
                //}
            }
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                if (o.Input.Equals(o.Output))
                {
                    Console.WriteLine("Input and output must not be the same");
                    return;
                }

                var inputFolder = o.Input;
                var outputFolder = o.Output;
                Console.WriteLine(inputFolder);
                Console.WriteLine(outputFolder);
                foreach (var file in Directory.EnumerateFiles(inputFolder, "*.upk"))
                {
                    var inputFileName = Path.GetFileName(file);
                    var outputFilePath = Path.Combine(outputFolder, inputFileName);
                    if (File.Exists(outputFilePath))
                    {
                        Console.Error.WriteLine("File is already decrypted.");
                        continue;
                    }

                    new FileInfo(outputFilePath).Directory.Create();
                    //Console.WriteLine($"Processing: {inputFileName}");
                    try
                    {
                        ProcessFile(file, outputFilePath);
                    }
                    catch (InvalidDataException e)
                    {
                        Console.WriteLine("Exception caught: {0}", e);
                    }
                    catch (OutOfMemoryException e)
                    {
                        Console.WriteLine("Exception caught: {0}", e);
                    }
                }

                for (int i = 0; i < AESKeys.KeyList.Count; i++)
                {
                    Console.WriteLine("Key{0} got used {1} times", i + 1, AESKeys.KeyListSuccessCount[i]);
                }

                Console.WriteLine("Finished!");
            });
        }
    }
}
