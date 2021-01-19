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
using System.IO;

namespace Products4Nerdz
{
    public partial class MainWindow : Window
    {
        private bool signedIn = false;
        private HTTP http = new HTTP();
        private string token = "";
        private List<ProductModel> productsList;
        private ProductModel selectedProduct;
        public MainWindow()
        {
            InitializeComponent();
            this.FetchProducts();
            try
            {
                string text = File.ReadAllText("token.txt");
                this.AuthUser(text);
            } catch(Exception)
            {

            }
        }

        private async void AuthUser(string TOKEN)
        {
            try
            {
                string res = await this.http.GetAsync("http://192.168.0.184:5000/user/authenticate", TOKEN);
                AuthResponseModel response = JsonConvert.DeserializeObject<AuthResponseModel>(res);
                if (response.status)
                {
                    this.UserIsValid(response.username);
                    this.token = TOKEN;
                }
            } catch(Exception) { }
        }

        private void UserIsValid(string username)
        {
            SidebarTitle.Text = "Hi " + username + "!";
            SignOutSidebarButton.Content = "Sign Out";
            AddProductSidebarButton.Visibility = Visibility.Visible;
            this.signedIn = true;
        }

        private async void FetchProducts()
        {
            try
            {
                string res = await http.GetAsync("http://192.168.0.184:5000/products");
                this.productsList = JsonConvert.DeserializeObject<List<ProductModel>>(res);
                foreach (var product in productsList)
                {
                    ProductsListBox.Items.Add(product.title);
                }
                TitleText.Text = "Products Available | " + this.productsList.Count().ToString();
            } catch(Exception) { }
        }

        private void ProductsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductModel product = productsList[ProductsListBox.SelectedIndex];
            selectedProduct = product;
            ProductSection.Visibility = Visibility.Visible;
            ProductsSection.Visibility = Visibility.Collapsed;
            TitleProductSection.Text = product.title;
            if (!signedIn)
                SendMailButton.Visibility = Visibility.Collapsed;
            else
                SendMailButton.Visibility = Visibility.Visible;
            DescriptionProductSection.Inlines.Add(new Run(product.description));
            try
            {
                ProductsListBox.SelectedIndex = -1;
            } catch(Exception) { }
        }

        private void ProductsSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            ProductSection.Visibility = Visibility.Collapsed;
            ProductsSection.Visibility = Visibility.Visible;
        }

        private void SignOutSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.signedIn)
            {
                SidebarTitle.Text = "Products4Nerdz";
                SignOutSidebarButton.Content = "Sign In";
                AddProductSidebarButton.Visibility = Visibility.Collapsed;
                this.signedIn = false;
                File.WriteAllText("token.txt", "");
                SendMailButton.Visibility = Visibility.Collapsed;
            } else
            {
                HomeContainer.Visibility = Visibility.Collapsed;
                SignInContainer.Visibility = Visibility.Visible;
            }
        }

        private void LoginBarHomeButton_Click(object sender, RoutedEventArgs e)
        {
            HomeContainer.Visibility = Visibility.Visible;
            SignInContainer.Visibility = Visibility.Collapsed;
        }

        private void LoginBarSignupButton_Click(object sender, RoutedEventArgs e)
        {
            LoginBarSignupButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121212"));
            LoginBarSigninButtpn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3e3e3e"));
            SignUpSection.Visibility = Visibility.Visible;
            SignInSection.Visibility = Visibility.Collapsed;
        }

        private void LoginBarSigninButtpn_Click(object sender, RoutedEventArgs e)
        {
            LoginBarSigninButtpn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121212"));
            LoginBarSignupButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3e3e3e"));
            SignUpSection.Visibility = Visibility.Collapsed;
            SignInSection.Visibility = Visibility.Visible;
        }

        private void SignInSectionButton_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrWhiteSpace(SignInSectionPassword.Password) || String.IsNullOrWhiteSpace(SignInSectionUsername.Text))
            {
                MessageBox.Show("Please fill required fields!");
            } else
            {
                UserLogInRequestModel user = new UserLogInRequestModel { password = SignInSectionPassword.Password, username = SignInSectionUsername.Text };
                string json = JsonConvert.SerializeObject(user);
                this.UserSignIn(json);
            }
        }

        private async void UserSignIn(string body)
        {
            string res = await this.http.PostAsync("http://192.168.0.184:5000/user/login", body);
            UserLoginResponseModel response = JsonConvert.DeserializeObject<UserLoginResponseModel>(res);
            if (response.status)
            {
                SignInContainer.Visibility = Visibility.Collapsed;
                HomeContainer.Visibility = Visibility.Visible;
                File.WriteAllText("token.txt", response.token);
                SignInSectionUsername.Text = "";
                SignInSectionPassword.Password = "";
                this.AuthUser(response.token);
            }
            else
                MessageBox.Show("User is not valid!");
            
        }

        private void SignUpSectionButton_Click(object sender, RoutedEventArgs e)
        {
            string username = SignUpSectionUsername.Text;
            string email = SignUpSectionEmail.Text;
            string password= SignUpSectionPassword.Password;
            UserSignUpRequestModel user = new UserSignUpRequestModel { email = email, username = username, password = password };
            bool userIsValid = true;
            if(String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(password))
            {
                userIsValid = false;
                MessageBox.Show("Please fill required fields!");
            } else
            {
                if (!this.http.IsValidEmail(email))
                {
                    userIsValid = false;
                    MessageBox.Show("Please enter a valid email!");
                } else
                {
                    if (username.Length > 8 || username.Length < 3)
                    {
                        userIsValid = false;
                        MessageBox.Show("Username should be between 3 and 8 characters!");
                    }
                }
            }
            if (userIsValid)
                this.UserSignUp(JsonConvert.SerializeObject(user));
        }

        private async void UserSignUp(string body)
        {
            string res = await this.http.PostAsync("http://192.168.0.184:5000/user/register", body);
            UserLoginResponseModel response = JsonConvert.DeserializeObject<UserLoginResponseModel>(res);
            if (!response.status)
                MessageBox.Show(response.msg);
            else
            {
                SignInContainer.Visibility = Visibility.Collapsed;
                HomeContainer.Visibility = Visibility.Visible;
                File.WriteAllText("token.txt", response.token);
                SignUpSectionUsername.Text = "";
                SignUpSectionEmail.Text = "";
                SignUpSectionPassword.Password = "";
                this.AuthUser(response.token);
            }
        }

        private void SendMailButton_Click(object sender, RoutedEventArgs e)
        {
            TitleProductSection.Text = "Sending...";
            SendMailRequestModel bodyObj = new SendMailRequestModel { email = selectedProduct.user, product = selectedProduct.title };
            string json = JsonConvert.SerializeObject(bodyObj);
            this.SendMail(json);
        }

        private async void SendMail(string BODY)
        {
            string res = await this.http.PostAsync("http://192.168.0.184:5000/email", BODY, "application/json", "POST", token);
            UserLoginResponseModel response = JsonConvert.DeserializeObject<UserLoginResponseModel>(res);
            if (response.status)
                MessageBox.Show("Email sent successfuly!");
            else
                MessageBox.Show("Error occured!");
            TitleProductSection.Text = selectedProduct.title;
        }

        private void ProductsReload_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "Reloading...";
            try
            {
                ProductsListBox.Items.Clear();
            } catch(Exception) { }
            this.FetchProducts();
        }

        private void AddProductSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow win = new AddProductWindow(this.token);
            win.Show();
        }
    }
}
