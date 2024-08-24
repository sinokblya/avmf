using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.CompilerServices;

public struct Structure
{
    public int a;
    public int b;
    public double result;
}

class PipeServer
{
    public static PriorityQueue<Structure, int> dataQueue = new PriorityQueue<Structure, int>();
    public static Mutex mutex = new Mutex();
    public static Mutex mutFile = new Mutex();
    public static int count = 0;
    public static string path = "C:\\Users\\USer\\Documents\\ЭВМ и периферийные устройства\\Lab3\\Client\\bin\\Debug\\net6.0\\Client.exe";

    public static async Task Main()
    {
        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken token = source.Token;
        StreamWriter File = new StreamWriter("output.txt", true);

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            source.Cancel();
        };

        try
        {
            await Task.WhenAll(SenderTask(token), ReceiverTask(token)); ;
        }
        catch(Exception error)
        {
            Console.WriteLine(error.Message);
        }
        finally
        {
            File.Close();
        }

        Task SenderTask(CancellationToken token)
        {
            return Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    int _n, _m, _priority;
                    Console.Write("Возведите a, b, приворитет: ");
                    string? res = Console.ReadLine();
                    if (res == null)
                    {
                        Console.WriteLine("Непрямой возвод");
                    }
                    else
                    {
                        string[] parse = res.Split(' ');
                        if (parse.Length == 2)
                        {
                            _n = int.Parse(parse[0]);
                            _m = int.Parse(parse[1]);
                            _priority = 0;
                        }
                        else
                        {
                            _n = int.Parse(parse[0]);
                            _m = int.Parse(parse[1]);
                            _priority = int.Parse(parse[2]);
                        }
                        Structure data = new Structure
                        {
                            a = _n,
                            b = _m,
                        };
                        mutex.WaitOne();
                        dataQueue.Enqueue(data, _priority);
                        mutex.ReleaseMutex();
                    }
                }
            });
        }

        Task ReceiverTask(CancellationToken token)
        {
            return Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Structure st;
                    int pr;
                    mutex.WaitOne();
                    bool flag = dataQueue.TryDequeue(out st, out pr);
                    mutex.ReleaseMutex();
                    if (flag)
                    {
                        ClientConnect(st, token);
                    }
                }
            });
        }

        async void ClientConnect(Structure st, CancellationToken token)
        {
            try
            {
                byte[] dataBytes = new byte[Unsafe.SizeOf<Structure>()];
                Unsafe.As<byte, Structure>(ref dataBytes[0]) = st;
                NamedPipeServerStream pipeServer = new($"channel{count}", PipeDirection.InOut);
                Console.WriteLine("Годим подрубання до служыльнику-тово...");
                Process myProcess = new Process();
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.FileName = path;
                myProcess.StartInfo.Arguments = $"channel{count}";
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();
                await pipeServer.WaitForConnectionAsync();
                Console.WriteLine("Клиен подрубльоной");
                await pipeServer.WriteAsync(dataBytes, 0, dataBytes.Length);
                byte[] receivedBytes = new byte[Unsafe.SizeOf<Structure>()];
                if (await pipeServer.ReadAsync(receivedBytes, 0, receivedBytes.Length) == receivedBytes.Length)
                {
                    st = Unsafe.As<byte, Structure>(ref receivedBytes[0]);
                }
                mutFile.WaitOne();
                Console.WriteLine($"a = {st.a}; b = {st.b}; приворитет = {0}; наследок = {st.result}");
                mutFile.ReleaseMutex();
                pipeServer.Close();
                count++;
                await myProcess.WaitForExitAsync(token);
            }
            catch (Exception) { }
        }
    }
}
