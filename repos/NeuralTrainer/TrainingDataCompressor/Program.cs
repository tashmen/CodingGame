using System;
using System.IO;
using System.IO.Compression;

namespace TrainingDataCompressor
{
    class Program
    {
        public static string pathToCodingGame = "C:\\Users\\jnorc\\CodingGame\\";
        public static string pathToDataFolder = pathToCodingGame + "data\\";
        public static string pathToCompressedDataFolder = pathToCodingGame + "compressedData\\";
        public static string dataFileType = ".data";
        public static string compressedDataFileType = ".gzip";
        public static void Main(string[] args)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var inputMemoryStream = new MemoryStream())
                {
                    using (var inputStream = new FileStream(pathToDataFolder + "0" + dataFileType, FileMode.Open))
                    {
                        using (var outputStream = new FileStream(pathToCompressedDataFolder + "0" + compressedDataFileType, FileMode.Create))
                        {
                            using (var compressor = new GZipStream(outputStream, CompressionMode.Compress))
                            {
                                inputStream.CopyTo(compressor);
                            }
                        }

                        using (var outputStream = new FileStream(pathToCompressedDataFolder + "0" + compressedDataFileType, FileMode.Open))
                        {
                            outputStream.CopyTo(memoryStream);
                        }
                    }

                    using (var inputStream = new FileStream(pathToDataFolder + "0" + dataFileType, FileMode.Open))
                    {
                        inputStream.CopyTo(inputMemoryStream);
                    }

                    var inputBytes = inputMemoryStream.ToArray();


                    var compressedBytes = memoryStream.ToArray();
                    var str = Convert.ToHexString(compressedBytes);

                    
                    var bytes2 = HexUtil.ToBytes(str);
                    for(int i = 0; i< compressedBytes.Length; i++)
                    {
                        if(compressedBytes[i] != bytes2[i])
                        {
                            throw new Exception("Not Same");
                        }
                    }

                    var compressedmemoryStream = new MemoryStream(bytes2);
                    var outputMemoryStream = new MemoryStream();

                    using var decompressor = new GZipStream(compressedmemoryStream, CompressionMode.Decompress);
                    decompressor.CopyTo(outputMemoryStream);
                    var bytes3 = outputMemoryStream.ToArray();

                    //ByteLoader.LoadBytesToReader();
                    //bytes3 = ByteLoader.bytes;
                    for (int i = 0; i < inputBytes.Length; i++)
                    {
                        if (inputBytes[i] != bytes3[i])
                        {
                            throw new Exception("Not Same");
                        }
                    }

                    Console.Error.Write(str);
                
                    Console.ReadKey();

                }
            }
        }
    }
}
