using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JiraGUI;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
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

namespace JiraGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{

		public MainWindow()
		{
			InitializeComponent();

		}

		static async Task<Issue> RunASync(string inputKey)
		{
			var input = inputKey;

			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri("https://icsasoftware.atlassian.net/rest/");
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + BasicEncode());

				try
				{
					HttpResponseMessage response = await client.GetAsync("api/2/issue/BPC-" + input);
					var result = response.Content.ReadAsStringAsync().Result;
					Issue issue = JsonConvert.DeserializeObject<Issue>(result);
					Console.WriteLine("{0}\n\n{1}\n\n{2}", issue.Self, issue.Key, issue.MyFields.Description);
					response.EnsureSuccessStatusCode();
					return issue;
				}
				catch (HttpRequestException e)
				{
					Console.WriteLine("{0}", e);
					return null;
				}
			}
		}

		static public string BasicEncode()
		{
			var bytes = Encoding.UTF8.GetBytes("sam.jones@icsasoftware.com:Jira2014");
			var base64 = Convert.ToBase64String(bytes);
			return base64;

		}

		private void OnKeyDownHandler(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				ButtonClick(sender,e);
			}
		}

		private void ButtonClick(object sender, RoutedEventArgs e)
		{
			Issue result;

			string inputKey = txtInput.Text;
			RunASync(inputKey).ContinueWith(r =>
				{
					Console.WriteLine("first >" + r.Result);
					result = r.Result;
					Console.WriteLine(result);
					this.Dispatcher.Invoke(() =>
						{
							txtOutput.Text = result.Key + "\n" + result.MyFields.Summary;
							txtMore.Text = result.MyFields.Description;
							Clipboard.SetText(txtOutput.Text);
						});

			});
		}

		private void SelectAll(object sender, RoutedEventArgs e)
		{
			this.Dispatcher.Invoke(() =>
				                       {
										   //Unboxing.
										   ((TextBox)sender).SelectAll();
					                       ((TextBox)sender).Focus();
				                       });
		}

	}
}
