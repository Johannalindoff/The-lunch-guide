﻿using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace LunchGuide.Models
{
    public class DishMethods
    {
        public DishMethods() { }

        public List<DishModel> GetListOfDishes(int restaurantId, out string errormsg)
        {
            //Skapa SqlConnection
            SqlConnection dbConnection = new SqlConnection();

            //Koppling mot Sql server
            dbConnection.ConnectionString = @"Data Source =.\sqlexpress; Initial Catalog = test; Integrated Security = True";

            //sqlstring, hämta information om lunchmeny tillhörande relevant restaurang
            String sqlstring = "SELECT dm.DM_Restaurant, dm.DM_Date, d.Di_Id, d.Di_Name, sd.SD_Type FROM Tbl_DailyMenu as dm INNER JOIN Tbl_DailyMenuHasDish as dmhd ON dm.DM_Id = dmhd.DMHD_DM_Id INNER JOIN Tbl_Dish as d ON dmhd.DMHD_Di_Id = d.Di_Id INNER JOIN Tbl_DishHasSpecialDiet as hsd ON d.Di_Id = hsd.HSD_Di_Id INNER JOIN Tbl_SpecialDiet sd ON sd.SD_Id = hsd.HSD_SD_Id WHERE dm.DM_Restaurant = @id ORDER BY d.Di_Id ASC";

            SqlCommand dbCommand = new SqlCommand(sqlstring, dbConnection);

            dbCommand.Parameters.Add("id", SqlDbType.NVarChar, 30).Value = restaurantId;

            //Skapa en adapter
            SqlDataAdapter myAdapter = new SqlDataAdapter(dbCommand);
            DataSet myDS = new DataSet();
            List<DishModel> DishList = new List<DishModel>();

            try
            {
                dbConnection.Open();
                myAdapter.Fill(myDS, "DishTable");

                int i = 0;
                int count = 0;
                count = myDS.Tables["DishTable"].Rows.Count;

                if (count > 0)
                {
                    // Gå igenom alla rader som ska läggas till
                    while (i < count)
                    {
                        // Skapa en Dishmodel och lägg till det nuvarande id:et
                        DishModel dm = new DishModel();
                        dm.Id = Convert.ToInt16(myDS.Tables["DishTable"].Rows[i]["Di_Id"]);
                        dm.Date = Convert.ToDateTime(myDS.Tables["DishTable"].Rows[i]["DM_Date"]);


                        // Kolla om Dishlistan redan innehåller det nuvarande id:et
                        if (DishList.Any(d => d.Id == dm.Id))
                        {
                            // Kolla vart i Dishlistan det id:et finns
                            int listIndex = DishList.FindIndex(d => d.Id == dm.Id);

                            if (DishList[listIndex].Date == dm.Date)
                            {
                                // Om id:et har en lista som inte innehåller nuvarande allergi, lägg till det
                                if (!(DishList[listIndex].SpecialDiet.Contains(myDS.Tables["DishTable"].Rows[i]["SD_Type"].ToString())))
                                {
                                    DishList[listIndex].SpecialDiet.Add(myDS.Tables["DishTable"].Rows[i]["SD_Type"].ToString() + " ");
                                }
                            }    
                        }
                        // Om Dishlistan inte innehåller det nuvarande id:et, lägg till maträtten
                        else
                        {
                            dm.Dish = myDS.Tables["DishTable"].Rows[i]["Di_Name"].ToString();
                            dm.SpecialDiet = new List<String>();
                            dm.SpecialDiet.Add(myDS.Tables["DishTable"].Rows[i]["SD_Type"].ToString() + " ");

                            DishList.Add(dm);
                        }

                        // Kolla nästa rad
                        i++;
                    }
                    errormsg = "";
                    return DishList;
                }
                else
                {
                    errormsg = "Det går inte att hämta information om rätterna";
                    return (null);
                }
            }
            catch (Exception e)
            {
                errormsg = e.Message;
                return null;
            }
            finally
            {
                dbConnection.Close();
            }
        }
    }
}