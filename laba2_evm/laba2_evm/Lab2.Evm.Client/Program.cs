using System.IO.Pipes;
using System.Runtime.CompilerServices;

public struct Structure
{
    public int n;
    public int m;
}

class Client
{
    static void Main()
    {
        using (NamedPipeClientStream Client = new(".", "channel", PipeDirection.InOut))
        {
            Client.Connect();
            try
            {
                while (true)
                {
                    byte[] bytes = new byte[Unsafe.SizeOf<Structure>()];
                    Client.Read(bytes, 0, bytes.Length);
                    Structure receivedData = Unsafe.As<byte, Structure>(ref bytes[0]);
                    Console.WriteLine($"Received data: n = {receivedData.n}, m = {receivedData.m}");

                    receivedData.n *= receivedData.m;
                    byte[] modified_bytes = new byte[Unsafe.SizeOf<Structure>()];
                    Unsafe.As<byte, Structure>(ref modified_bytes[0]) = receivedData;
                    Client.Write(modified_bytes, 0, modified_bytes.Length);
                }
            }
            catch (Exception) { }
        }
    }
}
