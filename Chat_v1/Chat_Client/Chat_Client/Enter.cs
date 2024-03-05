using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Chat_Client
{
    public partial class Enter : Form
    {
        private Dictionary<string, int> serverDictionary;
        IPAddress serverIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        private TcpClient client = new TcpClient(); // Initialize the TcpClient

        //public Enter(TcpClient client)
        public Enter()

        {
            InitializeComponent();

            // Создание словаря для хранения серверов и портов
            serverDictionary = new Dictionary<string, int>();
            serverDictionary.Add("Server", 8888);
            textBoxServerPort.Text = "8888";
            // Получение IP-адреса сервера
            //textBoxServerIP.Text = "127.0.0.1";
            textBoxServerIP.Text = serverIP.ToString();
        }

        private void textBoxChooseServer_TextChanged(object sender, EventArgs e)
        {
            string serverName = textBoxChooseServer.Text;

            if (serverDictionary.ContainsKey(serverName))
            {
                int port = serverDictionary[serverName];
                textBoxServerPort.Text = port.ToString();
                // Получение IP-адреса сервера
                textBoxServerIP.Text = serverIP.ToString();
            }
            else
            {
                textBoxServerPort.Text = string.Empty; // Очищаем, если сервер не найден
                textBoxServerIP.Text = string.Empty; // Очищаем, если сервер не найден

            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            // Получение IP-адреса сервера из textBox2
            IPAddress serverIP;
            if (IPAddress.TryParse(textBoxServerIP.Text, out serverIP))
            {
                // Получение порта сервера
                int serverPort;
                if (int.TryParse(textBoxServerPort.Text, out serverPort))
                {
                    try
                    {
                        // Подключение к серверу
                        client.Connect(serverIP, serverPort);

                        MessageBox.Show("Подключено к серверу.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Создание экземпляра формы UserID
                        UserID userIDForm = new UserID(client);
                        userIDForm.Show();


                        this.Hide();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Не удалось подключиться к серверу: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректный номер порта.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите корректный IP-адрес сервера.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
