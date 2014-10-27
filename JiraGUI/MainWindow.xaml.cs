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
		
		//TODO Fully implement API class.
		//TODO Clean Up code (Use List instead of array, messy error catching etc.)
		//TODO Login
		//Public String used as the link behind lblLink
		public string link;
		public string[] ItemList = {"BPC","BPDL","BPMA","BPWD","FBA","BPAD"}; 
		
		public MainWindow()
		{
			InitializeComponent();
			//TODO I don't like this :(
			cmboProjects.ItemsSource = ItemList;
			cmboProjects.SelectedIndex = 0;
		}

		//ASync task for finding an issue
		//Accessed when focus is on the top searchbar and the enter button is pressed.
		static async Task<Issue> RunASync(string inputKey, string projectCode)
		{
			var input = inputKey;
			var project = projectCode;

			using (var client = new HttpClient())
			{
				//Defines the URI
				//Sets up headers for encoding and to send back JSON
				client.BaseAddress = new Uri("https://icsasoftware.atlassian.net/rest/");
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + BasicEncode());

				try
				{
					//Appends api link and the ticket number
					//TODO: Use String.Format Where Needed
					HttpResponseMessage response = await client.GetAsync("api/2/issue/" + project + "-" + input);
					var result = response.Content.ReadAsStringAsync().Result;
					Issue issue = JsonConvert.DeserializeObject<Issue>(result);
					response.EnsureSuccessStatusCode();
					return issue;
				}
				catch (HttpRequestException e)
				{
					return null;
				}
			}
		}

		//Very basic encoding for authentication
		static public string BasicEncode()
		{
			var bytes = Encoding.UTF8.GetBytes("sam.jones@icsasoftware.com:Jira2014");
			var base64 = Convert.ToBase64String(bytes);
			return base64;

		}

		//Handler for the "enter" button press.
		private void OnKeyDownHandler(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				ButtonClick(sender,e);
			}
		}

		//Called by OnKeyDownHandler
		//TODO: Change method names to be accurate
		//TODO: Check if you can find module names from API
		private void ButtonClick(object sender, RoutedEventArgs e)
		{
			Issue result;

			string inputKey = txtInput.Text;
			string projectCode = cmboProjects.SelectedItem.ToString();

			

			RunASync(inputKey,projectCode).ContinueWith(r =>
				{
					result = r.Result;

					if (result==null)
					{
						this.Dispatcher.Invoke(() =>
							                       {
								                       txtOutput.Text = "Please enter a valid ticket number.";
								                       txtMore.Text = "";
							                       });
					}
					else{
					this.Dispatcher.Invoke(() =>
						{
							txtOutput.Text = result.Key + "\n" + result.MyFields.Summary;
							txtMore.Text = result.MyFields.Description;
							link = "https://icsasoftware.atlassian.net/browse/" + result.Key;
							Clipboard.SetText(result.Key + " " + result.MyFields.Summary);

						});
					}
			});
		}

		//Double click handler for lblLink
		private void GoToTicket(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(link);

		}

		//Handles the selection of the main search bar.
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
