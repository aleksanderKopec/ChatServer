using System;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip = "127.0.0.1";
            int port = 12345;

            // TODO: Workout some way to make server check for valid host or ip
            // So far only ip is valid

            // This should propably be in separate method or even class.
            // TODO: Move arguments handling to other method/class
            if (args.Length == 2)
            {
                if (isValidIp(args[0])) { ip = args[0]; }
                else { Console.WriteLine("Please enter a valid ip adress"); return; }

                int intPortArg = Int32.Parse(args[1]);
                if (isPortValid(intPortArg)) { port = intPortArg; }
                else { Console.WriteLine("Please enter a valid port"); return; }
            }
            else if (args.Length == 0)
            {
                Console.WriteLine("No arguments given, assuming ip=127.0.0.1 and port=12345 ");
            }
            else
            {
                Console.WriteLine("Invalid number of arguments, try --help for usage");
            }


            Console.WriteLine($"Starting chat server with ip: {ip} and port: {port}");
            Server server = new Server(ip, port);
        }


        static bool isValidIp(string ip)
        {
            ip.Trim();

            foreach (char symbol in ip)
            {
                if (!Char.IsDigit(symbol) && (symbol != '.')) { return false; }
            }

            string[] ipArray = ip.Split('.');


            foreach (string octet in ipArray)
            {
                int intOctet = Int32.Parse(octet); 
                if (intOctet < 0 || intOctet > 255)
                {
                    return false;
                }
            }
            return true;
        }

        static bool isPortValid(int port)
        {
            if (port < 0 || port > 65535)
            {
                return false;
            }
            return true;
        }
    }
}
