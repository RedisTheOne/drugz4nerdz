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
using Newtonsoft.Json;
using Products4Nerdz.models;

namespace Products4Nerdz
{   
    public partial class AddProductWindow : Window
    {
        private HTTP http = new HTTP();
        private string token;
        public AddProductWindow()
        {
            InitializeComponent();
        }

        public AddProductWindow(string token)
        {
            InitializeComponent();
            this.token = token;
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text;
            string description = DescriptionTextBox.Text;
            if (String.IsNullOrWhiteSpace(title) || String.IsNullOrWhiteSpace(description))
                MessageBox.Show("Please add required values");
            else
            {
                AddProductRequestModel model = new AddProductRequestModel { title = title, description = description };
                string json = JsonConvert.SerializeObject(model);
                this.AddProduct(json);
            }
        }

        private async void AddProduct(string body)
        {
            string res = await this.http.PostAsync("http://192.168.0.184:5000/products/add", body, "application/json", "POST", token);
            MessageBox.Show("Product added!");
        }
    }
}
