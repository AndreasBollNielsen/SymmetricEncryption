using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;

namespace SymmetricEncryption
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int selection;
        int mode;
        double encryptionTime;
        double decryptionTime;
        SymmetricAlgorithm myAlgorithm = Aes.Create();

        public MainWindow()
        {
            InitializeComponent();
        }



        /// <summary>
        /// Get selection from combobox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAlgorithm(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            selection = comboBox.SelectedIndex;

        }


        /// <summary>
        /// return algorithm based on selected index on combobox
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private SymmetricAlgorithm GetAlgorithm(int index)
        {

            SymmetricAlgorithm al = null;
            switch (index)
            {
                case 0:
                    al = Aes.Create();
                    al.KeySize = 128;
                    break;
                case 1:
                    al = Aes.Create();
                    al.KeySize = 256;
                    break;
                case 2:
                    al = TripleDES.Create();
                    break;
            }

            return al;
        }


        /// <summary>
        /// Encrypt data from user input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Encrypt(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string data = plaintext.Text;
            
            //begin encryption
            using (myAlgorithm)
            {
                sw.Start();
                ICryptoTransform encryptor = myAlgorithm.CreateEncryptor();
                byte[] myEncryptedData;

                using (MemoryStream stream = new MemoryStream())
                {
                    using (CryptoStream crypto = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(crypto))
                        {
                            writer.Write(data);
                        }

                        myEncryptedData = stream.ToArray();
                    }


                    cipher.Text = Convert.ToBase64String(myEncryptedData);

                }

                //record elapsed time and show it to user
                sw.Stop();
                encryptionTime = sw.Elapsed.TotalMilliseconds;
                Encryption_label.Content = $"Encryption time: {encryptionTime.ToString()} ms";
            }
        }


        /// <summary>
        /// Generate keys and show them to user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateKey(object sender, RoutedEventArgs e)
        {
            myAlgorithm = GetAlgorithm(selection);
            myAlgorithm.Mode = GetMode();
            myAlgorithm.GenerateKey();
            myAlgorithm.GenerateIV();
            byte[] key = myAlgorithm.Key;
            byte[] iv = myAlgorithm.IV;
            string key_s = Convert.ToBase64String(key);
            string iv_s = Convert.ToBase64String(iv);


            key_textbox.Text = key_s;
            iv_textbox.Text = iv_s;
        }


        /// <summary>
        /// Decrypt data from cipher text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Decrypt(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();

            myAlgorithm.Key = Convert.FromBase64String(key_textbox.Text);
            myAlgorithm.IV = Convert.FromBase64String(iv_textbox.Text);

            //encrypt data based on selected algorithm
            using (myAlgorithm)
            {
                sw.Start();
                ICryptoTransform decryptor = myAlgorithm.CreateDecryptor();
                byte[] myEncryptedData = Convert.FromBase64String(cipher.Text);

                using (MemoryStream stream = new MemoryStream(myEncryptedData))
                {
                    using (CryptoStream crypto = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(crypto))
                        {
                            DecryptedPlainText.Text = reader.ReadToEnd();

                        }
                    }
                }
                
                //record elapsed time
                sw.Stop();
                decryptionTime = sw.Elapsed.TotalMilliseconds;
                Decryption_label.Content = $"Decryption time: {decryptionTime.ToString()} ms";
            }
        }

        /// <summary>
        /// Set mode based on index number selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeMode(object sender, SelectionChangedEventArgs e)
        {
            var modes = (ComboBox)sender;
            mode = modes.SelectedIndex;

        }


        /// <summary>
        /// return ciphermode based on selection
        /// </summary>
        /// <returns></returns>
        private CipherMode GetMode()
        {
            CipherMode CipherMode = CipherMode.CBC;
            switch (mode)
            {
                case 0:
                    CipherMode = CipherMode.CBC;
                    break;
                case 1:
                    CipherMode = CipherMode.ECB;
                    break;
                case 2:
                    CipherMode = CipherMode.CFB;
                    break;


            }
            return CipherMode;
        }
    }
}
