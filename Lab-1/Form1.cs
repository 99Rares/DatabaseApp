using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Lab_1
{
    public partial class Form1 : Form
    {
        //private readonly string connectionString = "Data Source=RARES-DAN-ASUS\\SQLEXPRESS;Initial Catalog=VideoRental;" + "Integrated Security=true";
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
        private readonly string vaterTabelle = ConfigurationManager.AppSettings["vaterTable"];
        private readonly string kindTabelle = ConfigurationManager.AppSettings["kindTable"];
        //private readonly string actorQuerry = "select * from Actor";
        //private readonly string awardQuerry = "select * from ActorAward";

        private DataRelation relation;
        private BindingSource bindVater;
        private BindingSource bindKind;
        private DataSet dataSet;

        List<TextBox> tb = new List<TextBox>();


        public Form1()
        {
            InitializeComponent();
        }

        private void addActorTable()
        {
            string query = "SELECT * FROM [" + vaterTabelle + "]";
            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connectionString);
            dataAdapter.Fill(dataSet, vaterTabelle);
        }

        private void addActorAwardTable()
        {

            string query = "select * from [" + kindTabelle + "]";
            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connectionString);
            dataAdapter.Fill(dataSet, kindTabelle);
        }
        private void addRelation()
        {
            var primaryKey = findeSchlussel(vaterTabelle, "primary");
            var foreignKey = findeSchlussel(kindTabelle, "foreign");

            relation = new DataRelation("FK_" + vaterTabelle + "_" + kindTabelle, dataSet.Tables[vaterTabelle].Columns[primaryKey], dataSet.Tables[kindTabelle].Columns[foreignKey]);
            dataSet.Relations.Add(relation);

            bindVater = new BindingSource
            {
                DataSource = dataSet,
                DataMember = vaterTabelle
            };
            bindKind = new BindingSource
            {
                DataSource = bindVater,
                DataMember = "FK_" + vaterTabelle + "_" + kindTabelle
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            dataSet = new DataSet();
            insertButton.Enabled = false;
            deleteButton.Enabled = false;
            UpdateButton.Enabled = false;
            try
            {
                connection.Open();
                addActorTable();
                addActorAwardTable();
                addRelation();


                connection.Close();
                actorData.DataSource = bindVater;
                textBox();
                Labels();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex.Message);
                connection.Close();
            }
        }

        private void actorData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            awardData.DataSource = bindKind;
            emptyBoxes();

        }

        private void awardData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            fillBoxes();

            insertButton.Enabled = true;
            deleteButton.Enabled = true;
            UpdateButton.Enabled = true;
        }
        private void fillBoxes()
        {
            emptyBoxes();
            var selectedRow = this.awardData.CurrentRow;

            for (int i = 0; i < tb.Count; i++)
            {
                tb[i].Text = selectedRow.Cells[i].Value.ToString();
            }
        }
        private void emptyBoxes()
        {
            for (int i = 0; i < tb.Count; i++)
            {
                tb[i].Text = "";
            }
        }
        private void textBox()
        {
            int distance = 0;
            for (int i = 0; i < findColumnNumber(kindTabelle); i++)
            {
                TextBox textBox = new TextBox
                {
                    Location = new Point(360, 300 + distance),
                    Width = 200,
                    Enabled = true
                };
                this.Controls.Add(textBox);

                tb.Add(textBox);
                distance += 33;
            }
        }
        private void Labels()
        {
            List<string> columns = findColumnNames(kindTabelle);
            int distance = 0;
            for (int i = 0; i < findColumnNumber(kindTabelle); i++)
            {
                Label label = new Label
                {
                    Location = new Point(260, 300 + distance),
                    Text = columns[i],
                    TextAlign = ContentAlignment.MiddleRight
                };
                this.Controls.Add(label);

                distance += 33;
            }
        }
        private string ColumnsToInsert()
        {
            string columns = "";
            for (int i = 0; i < tb.Count; i++)
            {
                if (i != tb.Count - 1)
                    if (dataSet.Tables[kindTabelle].Columns[i].DataType.ToString() == "System.DateTime")
                        columns = columns + "CONVERT(DATE, @textBox" + i.ToString() + ", 105),  ";
                    else
                        columns = columns + "@textBox" + i.ToString() + ", ";
                else
                    if (dataSet.Tables[kindTabelle].Columns[i].DataType.ToString() == "System.DateTime")
                    columns = columns + "CONVERT(DATE, @textBox" + i.ToString() + ", 105) ";
                else
                    columns = columns + "@textBox" + i.ToString();
            }

            return columns;
        }
        private void insertButton_Click(object sender, EventArgs e)
        {
            string columns = ColumnsToInsert();
            string insertQ = "insert into " + kindTabelle + " values (" + columns + ")";
            SqlConnection connection = new SqlConnection(connectionString);

            SqlCommand command = new SqlCommand(insertQ, connection);
            for (int i = 0; i < tb.Count; i++)
            {
                command.Parameters.AddWithValue("textBox" + i.ToString(), tb[i].Text);
            }

            //string id = idtext.Text;

            //sqlDataAdapter.InsertCommand = new SqlCommand(insertQ, connection);
            //sqlDataAdapter.InsertCommand.Parameters.Add("@ID", SqlDbType.VarChar, 50);
            //sqlDataAdapter.InsertCommand.Parameters["@ID"].Value = id;

            //sqlDataAdapter.InsertCommand.Parameters.Add("@Name", SqlDbType.VarChar, 50);
            //sqlDataAdapter.InsertCommand.Parameters["@Name"].Value = nametext.Text;

            //sqlDataAdapter.InsertCommand.Parameters.Add("@ActorID", SqlDbType.Int);
            //sqlDataAdapter.InsertCommand.Parameters["@ActorID"].Value = actorIDtext.Text;

            //sqlDataAdapter.InsertCommand.Parameters.Add("@Category", SqlDbType.VarChar, 50);
            //sqlDataAdapter.InsertCommand.Parameters["@Category"].Value = categorytext.Text;

            //sqlDataAdapter.InsertCommand.Parameters.Add("@Date", SqlDbType.Date);
            //sqlDataAdapter.InsertCommand.Parameters["@Date"].Value = dateText.Text;

            try
            {
                connection.Open();
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("New Row inserted");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                //MessageBox.Show("Row NOT inserted");
                MessageBox.Show("Row NOT inserted", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();

                dataSet.Tables[kindTabelle].Clear();
                addActorAwardTable();
            }

        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            var selectedRow = this.awardData.CurrentRow;
            var primaryKey = findeSchlussel(kindTabelle, "primary");
            var PK = selectedRow.Cells[primaryKey].Value;

            SqlConnection connection = new SqlConnection(connectionString);
            //sqlDataAdapter.DeleteCommand.Parameters["@ID"].Value = id.ToString();
            SqlCommand command = new SqlCommand("DELETE FROM " + kindTabelle + " WHERE " + primaryKey + " = @PK", connection);
            command.Parameters.AddWithValue("@PK", PK);

            try
            {
                connection.Open();
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Row deleted");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                //MessageBox.Show("Row NOT deleted");
                MessageBox.Show("Row NOT deleted", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                connection.Close();

                dataSet.Tables[kindTabelle].Clear();
                addActorAwardTable();
            }

        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {

            var selectedRow = this.awardData.CurrentRow;
            var primaryKey = findeSchlussel(kindTabelle, "primary");
            var PK = selectedRow.Cells[primaryKey].Value;

            string columns = ColumnsToUpdate();

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand("UPDATE " + kindTabelle + " SET " + columns + " WHERE " + primaryKey + " = @PK", connection);
            for (int i = 0; i < tb.Count; i++)
            {
                command.Parameters.AddWithValue("textBox" + i.ToString(), tb[i].Text);
            }
            command.Parameters.AddWithValue("@PK", PK);
            //sqlDataAdapter.UpdateCommand.Parameters["@IDog"].Value = id.ToString();

            //sqlDataAdapter.UpdateCommand.Parameters.Add("@ID", SqlDbType.VarChar, 50);
            //sqlDataAdapter.UpdateCommand.Parameters["@ID"].Value = idtext.Text;

            //sqlDataAdapter.UpdateCommand.Parameters.Add("@Name", SqlDbType.VarChar, 50);
            //sqlDataAdapter.UpdateCommand.Parameters["@Name"].Value = nametext.Text;

            //sqlDataAdapter.UpdateCommand.Parameters.Add("@ActorID", SqlDbType.Int);
            //sqlDataAdapter.UpdateCommand.Parameters["@ActorID"].Value = actorIDtext.Text;

            //sqlDataAdapter.UpdateCommand.Parameters.Add("@Category", SqlDbType.VarChar, 50);
            //sqlDataAdapter.UpdateCommand.Parameters["@Category"].Value = categorytext.Text;

            //sqlDataAdapter.UpdateCommand.Parameters.Add("@Date", SqlDbType.Date);
            //sqlDataAdapter.UpdateCommand.Parameters["@Date"].Value = dateText.Text;

            try
            {
                connection.Open();
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Row updated");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                //Console.ReadLine();
                //MessageBox.Show("Row NOT updated");
                MessageBox.Show("Row NOT updated", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

              
            }
            finally
            {
                connection.Close();

                dataSet.Tables[kindTabelle].Clear();
                addActorAwardTable();
            }
        }
        private string ColumnsToUpdate()
        {
            string columns = "";
            for (int i = 0; i < tb.Count; i++)
            {
                if (i != tb.Count - 1)
                    if (dataSet.Tables[kindTabelle].Columns[i].DataType.ToString() == "System.DateTime")
                        columns = columns + dataSet.Tables[kindTabelle].Columns[i].ColumnName.ToString() + " = CONVERT(DATE, @textBox" + i.ToString() + ", 105), ";
                    else
                    {
                        columns = columns + dataSet.Tables[kindTabelle].Columns[i].ColumnName.ToString() + " = @textBox" + i.ToString() + ", ";
                        //MessageBox.Show(dataSet.Tables[kindTable].Columns[i].DataType.ToString());
                    }
                else
                    if (dataSet.Tables[kindTabelle].Columns[i].DataType.ToString() == "System.DateTime")
                    columns = columns + dataSet.Tables[kindTabelle].Columns[i].ColumnName.ToString() + " = CONVERT(DATE, @textBox" + i.ToString() + ", 105) ";
                else
                {
                    columns = columns + dataSet.Tables[kindTabelle].Columns[i].ColumnName.ToString() + " = @textBox" + i.ToString();
                    //MessageBox.Show(dataSet.Tables[kindTable].Columns[i].DataType.ToString());
                }
            }

            return columns;
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tb.Count; i++)
            {
                tb[i].Text = "";
            }
        }
        private string findeSchlussel(string tableName, string keyType)
        {
            string keyName = "";
            string query = "";

            if (keyType == "primary")
            {
                query = "select c.column_name " +
                        "from information_schema.table_constraints t " +
                        "join information_schema.constraint_column_usage c " +
                        "on c.constraint_name = t.constraint_name " +
                        "where c.table_name = '" + tableName + "' and t.constraint_type = '" + keyType + " key'";

            }
            if (keyType == "foreign")
            {
                query = "select k.column_name " +
                        "from information_schema.key_column_usage k " +
                        "inner join information_schema.referential_constraints r " +
                        "on k.constraint_name = r.constraint_name " +
                        "inner join information_schema.constraint_column_usage c " +
                        "on c.constraint_name = r.unique_constraint_name " +
                        "where k.table_name = '" + kindTabelle + "' and c.table_name = '" + vaterTabelle + "'";

            }


            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            using (SqlDataReader reader = sqlCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    keyName = (string)reader[0];
                }
            }
            sqlConnection.Close();

            return keyName;
        }
        private int findColumnNumber(string name)
        {
            return dataSet.Tables[name].Columns.Count;
        }
        private List<string> findColumnNames(string tableName)
        {
            List<string> columns = new List<string>();

            for (int i = 0; i < findColumnNumber(tableName); i++)
            {
                columns.Add(dataSet.Tables[tableName].Columns[i].ColumnName);
            }
            return columns;
        }
    }
}
