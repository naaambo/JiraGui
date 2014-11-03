using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace JiraGUI
{
	/// <summary>
	/// Interaction logic for LoginWinodw.xaml
	/// </summary>
	public partial class LoginWindow : MetroWindow
	{

		public LoginWindow()
		{
			InitializeComponent();
			CheckLogin();
		}

		private void CheckLogin()
		{
			string test = ReadFileStreamReader();

			if (!String.IsNullOrEmpty(test))
			{
				CallLoginAsync(test);
			}

		}

		private void OnKeyDownHandler(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				Button_Click(sender,e);
			}
		}

		public void Button_Click(object sender, RoutedEventArgs e)
		{

			string encodedString = BasicEncode(txtUsername.Text, txtPassword.Password);
			CallLoginAsync(encodedString);
		}

		public async Task<string> CheckLogin(string encodedAuth)
		{
			using (var client = new HttpClient())
			{
				//Defines the URI
				//Sets up headers for encoding and to send back JSON
				client.BaseAddress = new Uri("https://icsasoftware.atlassian.net/rest/");
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + encodedAuth);

				try
				{
					HttpResponseMessage response = await client.GetAsync("api/2/myself");
					var result = response.EnsureSuccessStatusCode().StatusCode.ToString();
					return result;
				}
				catch (HttpRequestException)
				{
					return "User is unauthorized or not found, please check your Username and Password";
				}
			}
		}

		private void CallLoginAsync(string encodedString)
		{
			CheckLogin(encodedString).ContinueWith(r =>
			{
				if (r.Result == "OK")
				{
					this.Dispatcher.Invoke(() =>
					{
						if (chkRemember.IsChecked.GetValueOrDefault())
						{
							WriteFileStreamReader(encodedString);
						}
						MainWindow window = new MainWindow();
						this.Close();
						window.Show();
					}
						);
				}
				else
				{
					MessageBox.Show(r.Result);
				}
			});

		}

		static public string BasicEncode(string username, string password)
		{
			var bytes = Encoding.UTF8.GetBytes(username + ":" + password);
			var base64 = Convert.ToBase64String(bytes);
			return base64;
		}

		static string ReadFileStreamReader()
		{
			string text;

			string input = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			//Would be much more readable like:
			// System.Reflection.Assembly.GetExecutingAssembly().Location to a variable and then using that

			//Opens up a new StreamReader and reads line by line
			using (StreamReader sourceReader = File.OpenText(input + "/login.txt"))
			{
				text = sourceReader.ReadLine();
			}

			return text;
		}

		public async void WriteFileStreamReader(string encodedString)
		{

			string output = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

			using (StreamWriter sourceWriter = new StreamWriter(output + "/login.txt"))
			{
				await sourceWriter.WriteLineAsync(encodedString);
			}
		}
	}
}
