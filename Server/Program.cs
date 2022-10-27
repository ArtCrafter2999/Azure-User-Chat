using Server;
using System.Net;
using System.Net.Sockets;
using ServerClasses;

const int port = 8000;
TcpListener? listener = null;
try
{
    listener = new TcpListener(GetLocalIPAddress(), port);
    listener.Start();
    Console.WriteLine($"Waiting for connections...\nEnd-point: {GetLocalIPAddress()}");
    var consoleinput = new ConsoleOutput();
    while (true)
    {
        TcpClient tcpclient = listener.AcceptTcpClient();


        consoleinput.OnConnected();
        var factory = new ClientFactory()
        {
            Network = new NetModelsLibrary.Network(tcpclient.GetStream()),
            Listener = new RequestListener(),
            Respondent = new RequestResponse(),
            Handler = new RequestHandler(),
            //Client = new ServerClaasses.ClientObject(tcpclient),
            Notifyer = new ClientsNotifyer()
        };
        factory.Respondent.OnSuccess += consoleinput.OnSuccess;
        factory.Respondent.OnFailure += consoleinput.OnFailure;
        factory.Client.OnDisconected += consoleinput.OnDisconected;
        consoleinput.Bind(factory.Listener);
        var Client = factory.MakeClient();
        //Client.Listener.BeginListen();


        //Server.ClientObject clientObject = new Server.ClientObject(tcpclient);

        //// создаем новый поток для обслуживания нового клиента
        //clientObject.Start();
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    if (listener != null)
        listener.Stop();
}
Console.ReadLine();


IPAddress GetLocalIPAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            return ip;
        }
    }
    throw new Exception("No network adapters with an IPv4 address in the system!");
}