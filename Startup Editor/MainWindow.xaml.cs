using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;


namespace Startup_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            startupListBox.IsTextSearchEnabled = true;
            if (IsUserAdministrator() != true)
            {
                adminNoticeTextBlock.Text = "Warning! Not running as admin. Some functionality is lost!";
            }
            
            main();
              
        }


        private void main()
        {

            //Open the current user registry
            RegistryKey currentUserRun = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            //Add the current user's startup items to a list
            List<startItems> startupList = new List<startItems>();

            //Add each subvalue to the list
            foreach (string item in currentUserRun.GetValueNames())
            {
                startupList.Add(new startItems()
                {
                    name = item,
                    registryLocation = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                    currentUser = true,
                    executableLocation = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", item, null)
                });

            }

            //Grab all items in Current user RunOnce
            RegistryKey currentUserRunOnce = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", true);

            //Add each subvalue to the list
            foreach (string item in currentUserRunOnce.GetValueNames())
            {
                startupList.Add(new startItems()
                {
                    name = item,
                    registryLocation = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce",
                    currentUser = true,
                    executableLocation = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", item, null)
                });

            }

            //Now grab the local startup items
            RegistryKey localMachineRun = null;
            try
            {
                localMachineRun = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Cannot access Local Run", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //Add each subvalue to the list
            if(localMachineRun != null)
            {
                foreach (string item in localMachineRun.GetValueNames())
                {
                    startupList.Add(new startItems()
                    {
                        name = item,
                        registryLocation = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                        currentUser = false,
                        executableLocation = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", item, null)
                    });

                }
            }


            //Now grab the local startup runonce
            RegistryKey localMachineRunOnce = null;
            try
            {
                localMachineRunOnce = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", true);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Cannot access Local RunOnce", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            

            //Add each subvalue to the list
            if(localMachineRunOnce != null)
            {
                foreach (string item in localMachineRunOnce.GetValueNames())
                {
                    startupList.Add(new startItems()
                    {
                        name = item,
                        registryLocation = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce",
                        currentUser = false,
                        executableLocation = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", item, null)
                    });

                }
            }
            


            //Set the name to be displayed in the listbox
            startupListBox.DisplayMemberPath = "name";


            //Now add all items in the list to the startup listbox
            foreach (startItems item in startupList)
            {
                startupListBox.Items.Add(new startItems
                {
                    name = item.name,
                    registryLocation = item.registryLocation,
                    currentUser = item.currentUser,
                    executableLocation = item.executableLocation
                });
            }
        }




        private void startupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            registryLocationTextBox.Text = ((startItems)startupListBox.SelectedItem).registryLocation;
            if(((startItems)startupListBox.SelectedItem).currentUser)
            {
                currentUserCheckBox.IsChecked = true;
            }
            else
            {
                currentUserCheckBox.IsChecked = false;
            }

            textBox.Text = ((startItems)startupListBox.SelectedItem).executableLocation;
        }



        private void deleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if(startupListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("You need to select at least one item");
                return;
            }

             foreach(startItems item in startupListBox.SelectedItems)
            {
                //Is this under current user
                if(item.currentUser)
                {
                    using (RegistryKey currentUserDelete = Registry.CurrentUser.OpenSubKey(item.registryLocation, true))
                    {
                        if (currentUserDelete == null)
                        {
                            // Key doesn't exist do nothing
                        }
                        else
                        {
                            currentUserDelete.DeleteValue(item.name);
                        }
                    }
                }
                else
                {
                    //Check to see if running as admin, return if not
                    if(!IsUserAdministrator())
                    {
                        MessageBox.Show("You have to be running as admin to delete this key.", "Error: Need admin rights", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    using (RegistryKey localUserDelete = Registry.LocalMachine.OpenSubKey(item.registryLocation, true))
                    {
                        if (localUserDelete == null)
                        {
                            // Key doesn't exist do nothing
                        }
                        else
                        {
                            localUserDelete.DeleteValue(item.name);
                        }
                    }
                }
            }
            
        }



        public bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            WindowsIdentity user = null;
            try
            {
                //get the currently logged in user
                user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            finally
            {
                if (user != null)
                    user.Dispose();
            }
            return isAdmin;
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}
