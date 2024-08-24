using System.IO.Pipes;
using System.Runtime.CompilerServices;

public struct Structure
{
    public int a;
    public int b;
    public double result;
}

class Client
{
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            using (NamedPipeClientStream Client = new(".", args[0], PipeDirection.InOut))
            {
                Client.Connect();
                try
                {
                    while (true)
                    {
                        byte[] bytes = new byte[Unsafe.SizeOf<Structure>()];
                        Client.Read(bytes, 0, bytes.Length);
                        Structure receivedData = Unsafe.As<byte, Structure>(ref bytes[0]);
                        Console.WriteLine($"Полученны даденны: a = {receivedData.a}, b = {receivedData.b}");
                        int a = receivedData.a;
                        int b = receivedData.b;
                        int n = 1000;

                        receivedData.result = TrapezoidalRule(a, b, n);
                        Console.WriteLine(receivedData.result);
                        byte[] modified_bytes = new byte[Unsafe.SizeOf<Structure>()];
                        Unsafe.As<byte, Structure>(ref modified_bytes[0]) = receivedData;
                        Client.Write(modified_bytes, 0, modified_bytes.Length);
                    }
                }
                catch (Exception) { }
            }
        }
    }
    static double Function(double x)
    {
        return -2 * x * x;
    }

    static double TrapezoidalRule(int a, int b, int n)
    {
        double h = (b - a) / Convert.ToDouble(n);
        double result = 0.5 * (Function(a) + Function(b));

        for (int i = 1; i < n; i++)
        {
            
            double x = a + i * h;
            result += Function(x);
        }

        result *= h;
        Console.WriteLine(result);
        return result;
    }
}

/*
public delegate double Func(double x);

public struct Result
{
    public int s;
    public double Res;

public struct Data
{
    public int s;
    public double chislo1;
    public double chislo2;
}

public class Integra
{
    public static double Integrate(Func f, double a, double b, double epsilon)
    {
        int n = 1;
        double previousResult, currentResult;

        currentResult = (b - a) * (f(a) + f(b)) / 2;

        do
        {
            n *= 2;
            double h = (b - a) / n;
            previousResult = currentResult;
            currentResult = 0.5 * (f(a) + f(b));
            for (int i = 1; i < n; i++)
            {
                currentResult += f(a + i * h);
            }

            currentResult *= h;

        } while (Math.Abs(currentResult - previousResult) > epsilon);

        return currentResult;
    }
}

public class PipeClient
{
    static void Main(string[] args)
    {
        if (args.Length < 1) 
        { 
            return; 
        }
        
        Func f = x => -2 * x * x;
        using (NamedPipeClientStream pipeClient = new(".", "channel", 
        pipeClient.Connect()));
        try 
        {
            pipeClient.Read(bytes);
        }
        finally { }
        
        Data received_data = Unsafe.As<byte, Data>(ref bytes[0]);
        Result result_data = new()
        {
            s = received_data.s,
            Res = Rule.Integrate(f, received_data.chislo1, received_data.chislo2, 0.0000001)
        };
        byte[] result_bytes = new byte[Unsafe.SizeOf<Result>()];
        Unsafe.As<byte, Result>(ref result_bytes[0]) = result_data;
        try
        {
            pipeClient.Write(result_bytes);
        }
        finally { }
        }
    }
}
*/
