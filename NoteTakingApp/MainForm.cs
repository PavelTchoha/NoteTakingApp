using System.Data.SqlClient;

namespace NoteTakingApp
{
    public partial class MainForm : Form
    {
        private string connectionString = @"Data Source=LAB109PC06\SQLEXPRESS; Initial Catalog=Notes; Integrated Security=True; Trusted_Connection=True; Encrypt=False;";
        public MainForm()
        {
            InitializeComponent();
            LoadNotesFromDatabase();
            InitializeListView();
        }

        private void InitializeListView()
        {
            listView1.View = View.Details;
            listView1.Columns.Add("Title", 150);
            listView1.Columns.Add("Timestamp", 200);
            listView1.FullRowSelect = true;

            listView1.SelectedIndexChanged += ListView1_SelectedIndexChanged;
        }

        private void LoadNotesFromDatabase()
        {
            listView1.Items.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT NoteID, Title, Timestamp FROM Notes ORDER BY Timestamp DESC";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new ListViewItem(reader["Title"].ToString())
                            {
                                Tag = reader["NoteID"]
                            };
                            item.SubItems.Add(Convert.ToDateTime(reader["Timestamp"]).ToString("g"));
                            listView1.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading notes: {ex.Message}");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Notes (Title, ContentNote, Timestamp) VALUES (@Title, @ContentNote, GETDATE())";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Title", textBox1.Text);
                        command.Parameters.AddWithValue("@ContentNote", richTextBox1.Text);
                        command.ExecuteNonQuery();
                    }
                    MessageBox.Show("Note saved successfully!");
                    LoadNotesFromDatabase();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save note: {ex.Message}");
                }
            }
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var noteId = listView1.SelectedItems[0].Tag;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "SELECT ContentNote FROM Notes WHERE NoteID = @NoteID";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@NoteID", noteId);
                            object content = command.ExecuteScalar();
                            if (content != null)
                            {
                                richTextBox1.Text = content.ToString();
                            }
                            else
                            {
                                richTextBox1.Text = "No content found for this note.";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to retrieve note content: {ex.Message}");
                    }
                }
            }
            else
            {
                richTextBox1.Text = string.Empty; // Clear the RichTextBox if no note is selected
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var confirmResult = MessageBox.Show("Are you sure you want to delete this note?",
                                     "Confirm Delete",
                                     MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    var noteId = listView1.SelectedItems[0].Tag;
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();
                            string query = "DELETE FROM Notes WHERE NoteID = @NoteID";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@NoteID", noteId);
                                command.ExecuteNonQuery();
                            }
                            MessageBox.Show("Note deleted successfully!");
                            LoadNotesFromDatabase();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to delete note: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a note to delete.");
            }
        }
    }
}