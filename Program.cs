using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Threading;
using dnlib.DotNet.Writer;

namespace Phoenix_Protector_Unpacker
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("phoenix unpacker.exe <filename>");
                Thread.Sleep(5000);
            }
            else
            {
                var mod = ModuleDefMD.Load(args[0]);
                foreach(TypeDef type in mod.Types)
                {
                    Console.WriteLine("Type:" +type.Name);
                    foreach(MethodDef method in type.Methods)
                    {
                        if (method.Body == null) continue;
                        Console.WriteLine("Method:" +method.Name);
                        method.Body.SimplifyBranches();
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                            {
                                var decryptedString = decryptString(method.Body.Instructions[i].Operand.ToString());
                                method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                method.Body.Instructions.Insert(i +1, new Instruction(OpCodes.Ldstr, decryptedString));
                                if (method.Body.Instructions[i + 2].OpCode == OpCodes.Call)
                                {
                                    method.Body.Instructions[i + 2].OpCode = OpCodes.Nop;
                                }
                                if (method.Body.Instructions[i - 1].OpCode == OpCodes.Call)
                                {
                                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                }
                                i += 1;
                            }
                            if(method.Body.Instructions[i].OpCode == OpCodes.Call)
                            {
                                if(method.Body.Instructions[i - 1].OpCode == OpCodes.Br)
                                {
                                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                }
                            }
                        }
                    }
                }
                ModuleWriterOptions writerOptions = new ModuleWriterOptions(mod);
                writerOptions.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
                writerOptions.Logger = DummyLogger.NoThrowInstance;
                NativeModuleWriterOptions NativewriterOptions = new NativeModuleWriterOptions(mod, false);
                NativewriterOptions.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
                NativewriterOptions.Logger = DummyLogger.NoThrowInstance;
                mod.Write("decrypted.exe",writerOptions);
                Console.ReadLine();
            }
        }
        public static string decryptString(string str)
	    {
		int length = str.Length;
        char[] array = new char[length];
		    for (int i = 0; i < array.Length; i++)
		    {
		    char c = str[i];
            byte b = (byte)((int)c ^ length - i);
            byte b2 = (byte)((int)(c >> 8) ^ i);
            array[i] = (char) ((int) b2 << 8 | (int) b);
            }
		return string.Intern(new string (array));
	    }
    }
}
