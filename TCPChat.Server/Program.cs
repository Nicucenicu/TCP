﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TCPChat.Server
{
     internal class Program
     {
          static TcpListener listener = new TcpListener(IPAddress.Any, 5050);
          static List<ConnectedClient> clients = new List<ConnectedClient>();
          static void Main(string[] args)
          {
               listener.Start();

               while (true)
               {
                    var client = listener.AcceptTcpClient();

                    Task.Factory.StartNew(() =>
                    {
                         var sr = new StreamReader(client.GetStream());
                         while (client.Connected)
                         {
                              var line = sr.ReadLine();
                              var nick = line.Replace("Login: ", "");

                              if (line.Contains("Login: ") && !string.IsNullOrWhiteSpace(nick))
                              {
                                   if (clients.FirstOrDefault(s => s.Name == nick) is null)
                                   {
                                        clients.Add(new ConnectedClient(client, nick));
                                        Console.WriteLine($"Conexiune noua: {nick}");
                                        break;
                                   }
                                   else
                                   {
                                        var sw = new StreamWriter(client.GetStream());
                                        sw.AutoFlush = true;
                                        sw.WriteLine("Utilizator deja existent");
                                        client.Client.Disconnect(false);
                                   }
                              }
                         }

                         while (client.Connected)
                         {
                              try
                              {
                                   var line = sr.ReadLine();

                                   SendToAllClients(line);

                                   Console.WriteLine(line);
                              }
                              catch (Exception ex)
                              {
                                   Console.WriteLine(ex.Message);
                              }
                         }
                    });
               }
          }

          private static void SendToAllClients(string message)
          {
               Task.Run(() =>
               {
                    for (int i = 0; i < clients.Count; i++)
                    {
                         try
                         {
                              if (clients[i].Client.Connected)
                              {
                                   var sw = new StreamWriter(clients[i].Client.GetStream());
                                   sw.AutoFlush = true;
                                   sw.WriteLine(message);
                              }
                              else
                              {
                                   Console.WriteLine($"{clients[i].Name} s-a deconectat");
                                   clients.RemoveAt(i);
                              }
                         }
                         catch (Exception ex)
                         {
                              Console.WriteLine(ex.Message);
                         }
                    }
               });
          }
     }
}