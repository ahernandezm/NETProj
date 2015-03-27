using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Data;
using System.Configuration;
using System.IO;
using System.Collections;
using System.Diagnostics;


namespace TrackingLineService.Datos
{
    class Datos
    {

        public OracleConnection cn;
        public OracleCommand cm;
        public OracleDataAdapter da;
        public OracleDataReader dr;
        public OracleDataReader dr2;
        String StrCon = ConfigurationManager.AppSettings["StrCon"];
        String StrCon2 = ConfigurationManager.AppSettings["StrConIAM"];

        private void Conecta()
        {
            try
            {
                cn = new OracleConnection(StrCon);
                cn.Open();
            }
            catch (Exception e)
            {

            }
        }

        private void Conecta2()
        {
            try
            {
                cn = new OracleConnection(StrCon2);
                cn.Open();
            }
            catch (Exception e)
            {

            }
        }

        private void Desconecta()
        {
            try
            {
                cn.Close();
            }
            catch (Exception e)
            {

            }
        }

        private void Desconecta2()
        {
            try
            {
                cn.Close();
            }
            catch (Exception e)
            {

            }
        }

        public DataTable GetPendingTlElements()
        {

            String Consulta = "SELECT * FROM CSI_TRANSACTIONS WHERE TRANSACTION_STATUS='T'";
            try
            {
                return ReturnDataTableByQuery(Consulta);

            }
            catch (Exception e)
            {

                return null;
            }
        }

        public DataTable GetPendingEelements()
        {

            String Consulta = "SELECT * FROM CSI_TRANSACTIONS WHERE TRANSACTION_STATUS='U'";
            try
            {
                return ReturnDataTableByQuery(Consulta);

            }
            catch (Exception e)
            {

                return null;
            }
        }


        public DataTable GetExisteCiudad(String ad_id)
        {
            String Consulta = " select atn_npa "
           + "from cyp_work.pb_listing@dblnk_ref "
          + "where listing_id = "
           + "     (select listing_id "
           + "        from cyp_work.pb_listing@dblnk_ref "
           + "       where listing_id = "
           + "             (select source_id "
           + "                from dlv_work.bc_source@dblnk_ref "
           + "               where business_id = "
           + "                     (select business_id "
           + "                        from dlv_work.bc_product@dblnk_ref "
           + "                       where ad_id =  " + ad_id.Substring(0, (ad_id.Length - 1))
           + "                         and effective_version_ind = 'Y' "
           + "                         and last_version_ind = 'Y') "
           + "                 and business_version = "
           + "                     (select max(business_version) "
           + "                        from dlv_work.bc_source@dblnk_ref "
           + "                       where business_id = "
           + "                             (select business_id "
           + "                                from dlv_work.bc_product@dblnk_ref "
           + "                               where ad_id =  " + ad_id.Substring(0, (ad_id.Length - 1))
           + "                                 and effective_version_ind = 'Y' "
           + "                                 and last_version_ind = 'Y')) "
           + "                 and source_code = 'LM') "
           + "         and last_version_ind = 'Y') "
           + " and last_version_ind = 'Y'";

            try
            {
                return ReturnDataTableByQuery(Consulta);

            }
            catch (Exception e)
            {

                return null;
            }
        }

        private DataTable ReturnDataTableByQuery(String Query)
        {

            DataTable dt;

            try
            {
                Conecta();
                cm = new OracleCommand(Query, cn);
                cm.Connection = cn;
                da = new OracleDataAdapter(cm);
                dt = new DataTable();
                da.Fill(dt);
                Desconecta();
                da.Dispose();
                return dt;
            }
            catch (Exception e)
            {

                return null;
            }
        }

        public String GetRealNumber(String ad_id, String advertiser_id)
        {
            try
            {
                DataTable dt;
                DataTable dt2;
                String TelReal = null;
                String Consulta1 = "select substr(attribute_value, 0, instr(attribute_value, '_') - 1) CONTENT_ID"
              + " from dlv_work.bc_product_attribute@dblnk_ref "
             + " where product_attribute_id = "
              + "     (select product_attribute_id "
              + "        from dlv_work.bc_product@dblnk_ref "
              + "       where bc_product_id = "
              + "             (SELECT UNIQUE bc_product_id "
              + "                FROM so_work.om_purchased_product@dblnk_ref om "
              + "               WHERE om.pd_product_code = 'TLINE'"
              + "                 AND l5_content_parent_id ="
              + "                     (SELECT prcsd_product_id "
              + "                        FROM so_work.om_purchased_product@dblnk_ref A,so_work.om_purchased_offer@dblnk_ref B"
              + "                       WHERE bc_product_id ="
              + "                             (SELECT bc_product_id "
              + "                                FROM dlv_work.bc_product@dblnk_ref"
              + "                               WHERE ad_id ="
              + "                                     SUBSTR(" + ad_id + ","
              + "                                            1,"
              + "                                            (LENGTH(" + ad_id + ") - 1))"
              + "                                 AND advertiser_id = " + advertiser_id
              + "                                 AND last_version_ind = 'Y' "
              + "                                 AND effective_version_ind = 'Y' "
              + "                                 AND product_status = 'I')   AND A.purchased_offer_id=B.purchased_offer_id "
              + "                                    AND A.PURCHASED_OFFER_VER=B.PURCHASED_OFFER_VER "
              + "                                    AND B.last_closed_version='Y'"
              + "                         AND product_status = 'I') "
              + "                 AND product_status = 'I') "
              + "         and effective_version_ind = 'Y' "
              + "         and last_version_ind = 'Y') "
              + " and attribute_code = 'TRKTELPRIN' ";




                Conecta();
                cm = new OracleCommand(Consulta1, cn);
                cm.Connection = cn;
                da = new OracleDataAdapter(cm);
                dt = new DataTable();
                da.Fill(dt);
                Desconecta();
                da.Dispose();


                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        String Consulta2 = "SELECT extractvalue(content_data, '//GeneralInfo/Description') TELREAL "
                         + "FROM acms_work.acms_content@dblnk_ref "
                         + "WHERE content_id =" + row["CONTENT_ID"].ToString();

                        Conecta();
                        cm = new OracleCommand(Consulta2, cn);
                        cm.Connection = cn;
                        da = new OracleDataAdapter(cm);
                        dt2 = new DataTable();
                        da.Fill(dt2);
                        Desconecta();
                        da.Dispose();

                        if (dt2.Rows.Count > 0)
                        {
                            foreach (DataRow row2 in dt2.Rows)
                            {
                                TelReal = row2["TELREAL"].ToString();

                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    return null;
                }
                return TelReal;
            }
            catch (Exception e)
            {
                return null;

            }


        }
        public Boolean UpdatetaTrlineLog(String transaction_id,String Teltrline)
        {
            Boolean bandera = false;
            try {
                int RegAfec = 0;
                
                String Query = "update ta_trline_log set procesado='Y' , tltrline=" + Teltrline + "  where transaction_id="+transaction_id;
                /*Actualizamos el registro*/
                Conecta();
                cm = new OracleCommand();
                cm.CommandText = Query;
                cm.Connection = cn;
                RegAfec = cm.ExecuteNonQuery();
                cm.Dispose();
                if(RegAfec>0)
                { bandera = true; }
 
            }catch(Exception){
                bandera = false;
            }
            return bandera;
        }

        public Boolean Actualizareg(String transaction)
        {
            try
            {
                int RegAfec = 0;
                Boolean bandera = false;
               /* String Query = "update csi_transactions "
         + " set listed_npa =" + uno + ", listed_cop =" + dos + ", listed_line_no = " + tres + " "
      + " where transaction_id = '" + transaction + "'";

                //Copiamos registro a log
                Conecta();
                cm = new OracleCommand();
                cm.CommandText = Query;
                cm.Connection = cn;
                RegAfec = cm.ExecuteNonQuery();
                cm.Dispose();

                if (RegAfec == 1)
                {*/
                    String Query =  "update csi_transactions "
                                    + " set transaction_status='I' "
                                    + " where transaction_id = '" + transaction + "'";

                    /*Copiamos registro a log*/
                    Conecta();
                    cm = new OracleCommand();
                    cm.CommandText = Query;
                    cm.Connection = cn;
                    RegAfec = cm.ExecuteNonQuery();
                    cm.Dispose();

                    if (RegAfec == 1) bandera = true;


                //}
                return bandera;
            }
            catch (Exception e)
            {
                return false;
            }


        }

        public Boolean ActualizaAcmsContent(String Advertiser, String Ad_id, String Tl,String DirIssue,String PrdCode)
        {
            try
            {
                DataTable dt;
                String BcProduct;
                Boolean Bandera = false;
               /* String Consulta = "SELECT UNIQUE bc_product_id "
                                    + "FROM so_work.om_purchased_product om "
                                    + "WHERE om.pd_product_code = 'TLINE' "
                                    + " AND l5_content_parent_id = "
                                    + "     (SELECT UNIQUE prcsd_product_id "
                                    + "        FROM so_work.om_purchased_product "
                                    + "       WHERE bc_product_id = "
                                    + "             (SELECT UNIQUE bc_product_id "
                                    + "                FROM dlv_work.bc_product "
                                    + "               WHERE ad_id = "
                                    + "                     SUBSTR(" + Ad_id + ", "
                                    + "                            1, "
                                    + "                            (LENGTH(" + Ad_id + ") - 1)) "
                                    + "                 AND advertiser_id = " + Advertiser + " "
                                    + "                 AND last_version_ind = 'Y' "
                                    + "                 AND effective_version_ind = 'Y' "
                                    + "                 AND product_status = 'I') "
                                    + "         AND product_status = 'I') "
                                    + " AND product_status = 'I' ";*/
                /*lnl 09/06/14*/
                String Consulta = "SELECT B.BC_PRODUCT_ID "
                + "FROM DLV_WORK.BC_PRODUCT B "
                + "WHERE B.L5_CONTENT_PARENT_ID = "
                + "        (SELECT A.BC_PRODUCT_ID "
                + "           FROM DLV_WORK.BC_PRODUCT A "
                + "          WHERE     A.ADVERTISER_ID = " + Advertiser
                + "                AND A.AD_ID = " + Ad_id
                + "                AND A.L5_DIRECTORY_ISSUE = " + DirIssue
                + "                 AND A.L5_DIRECTORY_CODE ='" + PrdCode + "' "
                + "                AND A.LAST_VERSION_IND = 'Y' "
               // + "                AND A.PRODUCT_CODE = 'DISPLAYITM' 
                + "                 and a.product_type = 'YELPRINT'"
                + "                AND    a.effective_version_ind='Y') "
                + "     AND B.PRODUCT_CODE = 'TLINE' "
                + "     AND B.LAST_VERSION_IND = 'Y' "
                + "     AND B.EFFECTIVE_VERSION_IND = 'Y' ";

                Conecta2();
                cm = new OracleCommand(Consulta, cn);
                cm.Connection = cn;
                da = new OracleDataAdapter(cm);
                dt = new DataTable();
                da.Fill(dt);
                Desconecta();
                da.Dispose();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {

                        BcProduct = row["bc_product_id"].ToString();

                        OracleConnection objConn = new OracleConnection(StrCon2);
                        OracleCommand objCmd = new OracleCommand();
                        objCmd.Connection = objConn;
                        objCmd.CommandText = "SP_CREATE_TL";
                        objCmd.CommandType = CommandType.StoredProcedure;
                        objCmd.Parameters.Add("p_advertiser_id", OracleType.VarChar).Value = Advertiser;
                        objCmd.Parameters.Add("p_bc_product_id", OracleType.VarChar).Value = BcProduct;
                        objCmd.Parameters.Add("p_tl_phone_num", OracleType.VarChar).Value = Tl.Replace("52", "");
                        try
                        {
                            objConn.Open();
                            int Valor = objCmd.ExecuteNonQuery();
                            if (Valor > 0)
                                Bandera = true;

                        }
                        catch (Exception ex)
                        {

                        }

                        objConn.Close();
                    }
                }
                return Bandera;
            }
            catch (Exception e)
            {
                return false;
            }



        }

        public String GetActualTelVersion(String ad_id)
        {
            try
            {
                DataTable dt;
                String Tel = String.Empty;
                String Consulta = "SELECT extractvalue(content_data, '//GeneralInfo/Description') UNO"
                                    + " FROM ACMS_WORK.ACMS_CONTENT "
                                    + " WHERE CONTENT_ID ="
                                    + "     (SELECT SUBSTR(ATTRIBUTE_VALUE, 0, INSTR(ATTRIBUTE_VALUE, '_') - 1)"
                                    + "        FROM DLV_WORK.BC_PRODUCT_ATTRIBUTE B"
                                    + "       WHERE B.PRODUCT_ATTRIBUTE_ID ="
                                    + "             (select PRODUCT_ATTRIBUTE_ID"
                                    + "                from dlv_work.bc_product a"
                                    + "               where a.product_CODE = 'TLINE'"
                                    + "                 AND l5_content_parent_id  ="
                                    + "                     (select bc_product_id"
                                    + "                        from dlv_work.bc_product"
                                    + "                       where ad_id = " + ad_id
                                    + "                         and last_version_ind = 'Y'"
                                    + "                         and effective_version_ind = 'Y')"
                                    + "                 AND LAST_VERSION_IND = 'Y'"
                                    + "                 AND EFFECTIVE_VERSION_IND = 'Y')"
                                    + "         AND ATTRIBUTE_CODE = 'TRKTELPRIN')  and last_version_ind = 'Y'";

                Conecta2();
                cm = new OracleCommand(Consulta, cn);
                cm.Connection = cn;
                da = new OracleDataAdapter(cm);
                dt = new DataTable();
                da.Fill(dt);
                Desconecta();
                da.Dispose();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Tel = row["UNO"].ToString();

                    }
                }
                return Tel;
            }
            catch (Exception e)
            {
                return null;
            }

        }
        /**/

        public DataTable GetPendingT()
        {

            String Consulta = "SELECT * FROM TA_TRLINE_LOG WHERE STATUS='N' AND PROCESADO='N'";
            try 
            {
                return ReturnDataTableByQuery(Consulta);

            }
            catch (Exception e)
            {

                return null;
            }
        }


        public DataTable GetTlineProcesados()
        {

            String Consulta = "SELECT * FROM TA_TRLINE_LOG WHERE STATUS='N' AND PROCESADO='Y' AND TRANSACTION_ID > 3";
            try 
            {
                return ReturnDataTableByQuery(Consulta);

            }
            catch (Exception e)
            {

                return null;
            }
        }

        public void FillTlineRequestTable()
    {
        using (cn = new OracleConnection(StrCon2))
        {
            cm = new OracleCommand();
            cm.Connection = cn;
            cm.CommandText = "PKG_TLINE.FILL_TLINE_REQUEST";
            cm.CommandType = CommandType.StoredProcedure;
            try {
                cn.Open();
                cm.ExecuteNonQuery();

            }catch(Exception e){
            }
        }

    }


        





    }
}
