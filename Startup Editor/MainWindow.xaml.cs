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
            startupListBox.Items.Clear();

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
                    executableLocation = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", item, null),
                    promptForDelete = false
                });

            }
            currentUserRun = null;

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
                    executableLocation = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", item, null),
                    promptForDelete = false
                });

            }
            currentUserRunOnce = null;

            //Grab the current run desktop enviroment
            RegistryKey currentUserDesktop = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", true);

            //Test to see if a custom desktop enviroment is set
            try
            {
                if (currentUserDesktop.GetValue("Shell").ToString().Length > 0)
                {
                    startupList.Add(new startItems
                    {
                        name = "Shell",
                        registryLocation = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon",
                        currentUser = true,
                        executableLocation = currentUserDesktop.GetValue("Shell").ToString(),
                        promptForDelete = true
                    });
                }
            }
            catch (Exception)
            {

            }
            currentUserDesktop = null;

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
                        executableLocation = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", item, null),
                        promptForDelete = false
                    });

                }
            }
            localMachineRun = null;

            //Grab the local shell
            try
            {
                localMachineRun = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", true);
                
            }
            catch (Exception)
            {

            }

            if(localMachineRun != null)
            {
                try
                {
                    if(localMachineRun.GetValue("Shell").ToString().Length > 0)
                    {
                        startupList.Add(new startItems
                        {
                            name = "Shell",
                            registryLocation = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon",
                            currentUser = false,
                            executableLocation = localMachineRun.GetValue("Shell").ToString(),
                            promptForDelete = true
                        });
                    }
                }
                catch (Exception)
                {

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
            localMachineRunOnce = null;


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
                    executableLocation = item.executableLocation,
                    promptForDelete = item.promptForDelete
                });
            }
            startupList = null;
        }




        private void startupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                registryLocationTextBox.Text = ((startItems)startupListBox.SelectedItem).registryLocation;
            }
            catch (Exception)
            {
                registryLocationTextBox.Text = "";
            }
            
            try
            {
                if (((startItems)startupListBox.SelectedItem).currentUser)
                {
                    currentUserCheckBox.IsChecked = true;
                }
                else
                {
                    currentUserCheckBox.IsChecked = false;
                }
            }
            catch (Exception)
            {
                currentUserCheckBox.IsChecked = false;
            }

            try
            {
                textBox.Text = ((startItems)startupListBox.SelectedItem).executableLocation;
            }
            catch (Exception)
            {
                textBox.Text = "";
            }
            
        }



        private void deleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if(startupListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("You need to select at least one item");
                return;
            }

            List<startItems> toDelete = new List<startItems>();
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
                            if(item.promptForDelete == true)
                            {
                                string messageResponse = MessageBox.Show("This is an important startup item. Are you sure you want to remove it?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning).ToString();
                                if(messageResponse == "Yes")
                                {
                                    currentUserDelete.DeleteValue(item.name);
                                    toDelete.Add(item);
                                }
                            }
                            else
                            {
                                currentUserDelete.DeleteValue(item.name);
                                toDelete.Add(item);
                            }
                            
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
                            if (item.promptForDelete == true)
                            {
                                string messageResponse = MessageBox.Show("This is an important startup item. Are you sure you want to remove it?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning).ToString();
                                if (messageResponse == "Yes")
                                {
                                    localUserDelete.DeleteValue(item.name);
                                    toDelete.Add(item);
                                }
                            }
                            else
                            {
                                localUserDelete.DeleteValue(item.name);
                                toDelete.Add(item);
                            }
                            
                        }
                    }
                }
            }

             //Now remove each item in the to be deleted
             foreach(startItems item in toDelete)
            {
                startupListBox.Items.Remove(item);
            }

             //Null any unsused 
            toDelete = null;
            
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            main();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            about aboutBox = new about();
            aboutBox.ShowDialog();
            aboutBox = null;
        }
    }
}
