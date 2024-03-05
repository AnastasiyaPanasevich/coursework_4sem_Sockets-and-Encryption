using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyRSALibrary_v1;

namespace Chat_Client
{
    public partial class UserID : Form
    {
        // Добавьте поле для хранения имени пользователя
        private string userName;
        private TcpClient client;
        private string combinedMessage;
        private string command;

        public UserID(TcpClient _client)
        {
            InitializeComponent();
            this.client = _client;
        }

        private void buttonID_Click(object sender, EventArgs e)
        {


            // Получение имени пользователя из textBoxUserID
            userName = textBoxID.Text;
            command = "L";
            //combinedMessage = $"{command};{userName};{openKey}";
            combinedMessage = $"{command};{userName}";
            try
            {

                // Получение потока сети из объекта TcpClient
                NetworkStream stream = client.GetStream();
                // Преобразование сообщения в массив байтов
                byte[] buffer = Encoding.UTF8.GetBytes(combinedMessage);

                // Отправка сообщения на сервер
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке логина: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Создание экземпляра формы Form1 с передачей имени пользователя

            Form1 form1 = new Form1(client, userName);
            form1.Show();

            // Закрытие формы
            this.Close();
        }

        private void textBoxID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                buttonID.PerformClick();
            }
        }
    }

}
