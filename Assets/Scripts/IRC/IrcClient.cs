/*
 * IrcClient.cs
 * Defines a class used for communicating with IRC servers.
 * 
 */
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using UnityEngine;
using System;

namespace TwitchIRCBot {

    public class IRCClient {

        public string userName;
        public string channel;

        private TcpClient _tcpClient;
        private StreamReader _inputStream;
        private StreamWriter _outputStream;

        public IRCClient(string ip, int port, string userName, string password, string channel) {
            try {
                //Variables
                this.userName = userName;
                this.channel = channel;

                //Set up the communications objects
                _tcpClient = new TcpClient(ip, port);
                _inputStream = new StreamReader(_tcpClient.GetStream());
                _outputStream = new StreamWriter(_tcpClient.GetStream());

                //Attempt to join the room
                _outputStream.WriteLine("PASS " + password);
                _outputStream.WriteLine("NICK " + userName);
                _outputStream.WriteLine("USER " + userName + " 8 * :" + userName);
                _outputStream.WriteLine("JOIN #" + channel);
                _outputStream.Flush();
            } catch (Exception ex) {
                //If anything fails, pass the exception to the next level.
                throw ex;
            }
        }

        public void SendIRCMessage(string message) {
            try {
                _outputStream.WriteLine(message);
                _outputStream.Flush();
            } catch (Exception ex) {
                throw ex;
            }
        }

        public void SendPublicChatMessage(string message) {
            try {
                SendIRCMessage(":" + userName + "!" + userName + "@" + userName +
                ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
            } catch (Exception ex) {
                throw ex;
            }
        }

        public void SendNamesCommand() {
            try {
                SendIRCMessage("NAMES #" + channel);
            } catch (Exception ex) {
                throw ex;
            }
        }

        //Because I'm a good IRC client...
        public void PartChannel() {
            try {
                SendIRCMessage("PART #" + channel);
            } catch (Exception ex) {
                throw ex;
            }
        }

        public string ReadMessage() {
            try {
                string message = _inputStream.ReadLine();
                return message;
            } catch (Exception ex) {
                throw ex;
            }
        }

    }
}