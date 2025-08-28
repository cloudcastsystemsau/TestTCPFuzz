using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpFuzzClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===============================================");
            Console.WriteLine("   TcpFuzzClient © Cloudcast Systems 2025");
            Console.WriteLine("   For support contact: support@cloudcastsystems.com");
            Console.WriteLine("===============================================\n");

            Console.Write("Enter server IP (e.g., 127.0.0.1): ");
            string ip = Console.ReadLine();

            Console.Write("Enter port (e.g., 30001): ");
            int port = int.Parse(Console.ReadLine());

            while (true)
            {
                Console.WriteLine("\n--- Select Test ---");
                Console.WriteLine("1. Send normal message");
                Console.WriteLine("2. Send invalid UTF-8");
                Console.WriteLine("3. Send partial message");
                Console.WriteLine("4. Send large message");
                Console.WriteLine("5. Rapid open/close");
                Console.WriteLine("6. Fuzz with garbage data");
                Console.WriteLine("7. Send fake TLS handshake");
                Console.WriteLine("0. Exit");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await SendMessage(ip, port, "hello\n");
                        break;
                    case "2":
                        await SendRawBytes(ip, port, new byte[] { 0xC3, 0x28 }); // Invalid UTF-8
                        break;
                    case "3":
                        await SendPartialMessage(ip, port);
                        break;
                    case "4":
                        await SendLargeMessage(ip, port);
                        break;
                    case "5":
                        await RapidOpenClose(ip, port);
                        break;
                    case "6":
                        await SendRawBytes(ip, port, Encoding.UTF8.GetBytes("💥🔥⚠️🐛\0\0\0\n"));
                        break;
                    case "7":
                        await SendFakeTLSHandshake(ip, port);
                        break;
                    case "0":
                        return;
                }
            }
        }

        static async Task SendMessage(string ip, int port, string message)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ip, port);
                var stream = client.GetStream();
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(bytes, 0, bytes.Length);
                Console.WriteLine("Sent: " + message.Replace("\n", "\\n"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task SendRawBytes(string ip, int port, byte[] data)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ip, port);
                var stream = client.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
                Console.WriteLine("Sent raw bytes: " + BitConverter.ToString(data));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task SendPartialMessage(string ip, int port)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ip, port);
                var stream = client.GetStream();
                byte[] part1 = Encoding.UTF8.GetBytes("partial-");
                byte[] part2 = Encoding.UTF8.GetBytes("message\n");

                await stream.WriteAsync(part1, 0, part1.Length);
                Console.WriteLine("Sent first half");
                await Task.Delay(1000);
                await stream.WriteAsync(part2, 0, part2.Length);
                Console.WriteLine("Sent second half");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task SendLargeMessage(string ip, int port)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ip, port);
                var stream = client.GetStream();

                string bigData = new string('A', 8192) + "\n";
                byte[] buffer = Encoding.UTF8.GetBytes(bigData);
                await stream.WriteAsync(buffer, 0, buffer.Length);
                Console.WriteLine($"Sent large message ({buffer.Length} bytes)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task RapidOpenClose(string ip, int port)
        {
            Console.Write("How many times? ");
            int count = int.Parse(Console.ReadLine());

            for (int i = 0; i < count; i++)
            {
                try
                {
                    using var client = new TcpClient();
                    await client.ConnectAsync(ip, port);
                    var stream = client.GetStream();
                    byte[] newline = Encoding.UTF8.GetBytes("\n");
                    await stream.WriteAsync(newline, 0, newline.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Attempt {i + 1}] Error: {ex.Message}");
                }
            }

            Console.WriteLine($"Completed {count} rapid connections.");
        }

        static async Task SendFakeTLSHandshake(string ip, int port)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ip, port);
                var stream = client.GetStream();

                // Minimal TLS 1.0 ClientHello preamble (truncated and malformed on purpose)
                byte[] tlsHello = new byte[]
                {
                    0x16, 0x03, 0x01, 0x00, 0x2e, // Handshake record, TLS 1.0, length
                    0x01, 0x00, 0x00, 0x2a,       // ClientHello, length
                    0x03, 0x03,                   // TLS 1.2 version
                    0x53, 0x43, 0x4f, 0x4d, 0x0D,0x0A,      // Random data
                    0x00                          // End (intentionally malformed)
                };

                await stream.WriteAsync(tlsHello, 0, tlsHello.Length);
                Console.WriteLine("Sent fake TLS handshake");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
