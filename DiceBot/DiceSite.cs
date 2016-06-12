﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security.Cryptography;
using System.Globalization;
using System.Web;
using System.Net;
using System.IO;
namespace DiceBot
{
    public abstract class DiceSite
    {
        bool reg = true;
        public bool register { get { return reg; } set { reg = value; } }
        protected string prox_host = "";
        protected int prox_port = 3128;
        protected string prox_username = "";
        protected string prox_pass = "";
        protected WebProxy Prox;
        public string[] Currencies = new string[] { "btc" };
        public double maxRoll { get; set; }
        string currency = "Btc";
        public string SiteURL { get; set; }
        public string Currency
        {
            get { return currency; }
            set { currency= value; CurrencyChanged();}}
            
        
        protected virtual void CurrencyChanged(){}

        protected cDiceBot Parent;
        public bool AutoWithdraw { get; set; }
        public bool AutoInvest { get; set; }
        public bool ChangeSeed {get;set;}
        public bool AutoLogin { get; set; }
        public decimal edge = 1;
        public string Name { get; protected set; }
        public double chance = 0;
        public double amount = 0;
        public double balance { get; protected set; }
        protected int bets = 0;
        protected double profit = 0;
        protected double wagered = 0;
        protected int wins = 0;
        protected int losses = 0;
        protected double siteprofit = 0;
        protected bool High = false;
        public string BetURL = "";
        public bool Tip { get; set; }
        public bool TipUsingName { get; set; }
        public bool GettingSeed { get; set; }


        public void PlaceBet(bool High, double amount, double chance)
        {
            Parent.updateStatus(string.Format("Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, chance, High ? "High" : "Low"));
            internalPlaceBet(High,amount, chance);
        }
        protected void FinishedBet(Bet newBet)
        {
            Parent.updateBalance(balance);
            Parent.updateBets(bets);
            Parent.updateLosses(losses);
            Parent.updateProfit(profit);
            Parent.updateWagered(wagered);
            Parent.updateWins(wins);
            Parent.AddBet(newBet);
            Parent.GetBetResult(balance, newBet);
                
        }
        protected abstract void internalPlaceBet(bool High,double amount, double chance);
        public abstract void ResetSeed();
        public abstract void SetClientSeed(string Seed);
        public virtual bool Invest(double Amount)
        {
            return true;

        }
        public virtual void Donate(double Amount)
        {

        }
        public bool Withdraw(double Amount, string Address)
        {
            Parent.updateStatus(string.Format("Withdrawing {0} {1} to {2}", Amount, currency, Address));
            return internalWithdraw(Amount, Address);
        }
        protected abstract bool internalWithdraw(double Amount, string Address);
        
        public abstract void Login(string Username, string Password, string twofa);
        
        public abstract bool Register(string username, string password);
        
        public abstract bool ReadyToBet();
        
        public virtual double GetLucky(string server, string client, int nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();
            
            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = /*nonce.ToString() + ":" + */client + ":" + nonce.ToString();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }
            
            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i+=charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);
                
                double lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }
        public static double sGetLucky(string server, string client, int nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();

            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = /*nonce.ToString() + ":" + */client + ":" + nonce.ToString();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                double lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }

        public abstract void Disconnect();
        
        public abstract void GetSeed(long BetID);
        public abstract void SendChatMessage(string Message);
        protected void ReceivedChatMessage(string Message)
        {
            Parent.AddChat(Message);
        }
        public virtual void SendTip(string User, double amount)
        {
            Parent.updateStatus("Tipping is not enabled for the current site.");
            
        }
        protected void finishedlogin(bool Success)
        {
            if (FinishedLogin!=null)
                FinishedLogin(Success);
        }

        public delegate void dFinishedLogin(bool LoggedIn);
        public event dFinishedLogin FinishedLogin;
        
        public virtual void SetProxy(string host, int port)
        {
            prox_host = host;
            prox_port = port;
            Prox = new WebProxy(prox_host, prox_port);
        }
        public virtual void SetProxy(string host, int port, string username, string password)
        {
            SetProxy(host, port);
            prox_username = username;
            prox_pass = password;
            Prox = new WebProxy(prox_host, prox_port);
            Prox.Credentials = new NetworkCredential(prox_username, prox_pass);
        }
        
    }
    public class PlaceBetObj
    {
        public PlaceBetObj(bool High, double Amount, double Chance)
        {
            this.High = High;
            this.Amount = Amount;
            this.Chance = Chance;
        }
        public bool High { get; set; }
        public double Amount { get; set; }
        public double Chance { get; set; }
    }

}
