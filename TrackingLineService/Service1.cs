using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Xml;
using System.Net;
using System.IO;
using TrackingLineService.Classes;
namespace TrackingLineService
{
    public partial class TrackingLineService : ServiceBase
    {
        /*Definicion de tiempo de ejecucion*/
        protected Timer Reloj = new Timer();
        private int Intervalo = 30000;

        public TrackingLineService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Reloj.Elapsed += new ElapsedEventHandler(On_Elapsed_Time);
            Reloj.Interval = this.Intervalo;
            Reloj.Enabled = true;
            Reloj.Start();
        }

        protected override void OnStop()
        {
        }

        /// <summary>
        /// Funcion ejecutora de logica para TrackingLine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eArgs"></param>
        public void On_Elapsed_Time(object sender, System.Timers.ElapsedEventArgs eArgs)
        {
            Reloj.Stop();

            try
            {                
                if ((DateTime.Now.Hour > 8) && (DateTime.Now.Hour < 21))
                {
                    Reloj.Stop();                                        
                    /*este proceso trabaje l tline no olvidar descomentarlo :)*/
                    ProccessT();
                    //realizado para anadir actualizar la API de agendize
                    //CorrigeTLINE();
                    //FillTlineRequestTable();

                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                Reloj.Start();
            }
        }

        public void FillTlineRequestTable()
        {
            Datos.Datos d = new Datos.Datos();
            d.FillTlineRequestTable();


        }


        public void ProcessE()
        {
            DataTable Eelements;
            using (StreamWriter w = new StreamWriter("c:\\log_trkLn.txt", true))
            {
                w.WriteLine("Buscando elementos con estatus E");
                Eelements = GetEelements();
                if (Eelements.Rows.Count > 0)
                {
                    foreach (DataRow row in Eelements.Rows)
                    {
                        String ad_id = row["PRD_IDENTIFICATION"].ToString().Substring(0, (row["PRD_IDENTIFICATION"].ToString().Length) - 1);
                        String TelActual = GetActualTelVersion(ad_id);
                        //String TelPrevious = GetPreviousTelVersion();
                    }


                }

            }
        }

        public DataTable GetPendingTlElements()
        {
            Datos.Datos datos = new Datos.Datos();
            return datos.GetPendingTlElements();
        }
        public Boolean GetExisteCiudadParaLada(String Lada)
        {

            Boolean existe = false;
            if (Lada != null)
            {
                existe = GetCitiesList(Lada.Trim());
            }

            return existe;
        }
        public String GetTkNumber(String Lada)
        {

            if (Lada != null)
            {


                return GetNumberFromList(Lada);

            }
            return null;
        }
        public Boolean GetCitiesList(String lada)
        {
            Boolean bandera = false;
            //requesting the particular web page
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://www.agendize.com/web/templates/12512922/getcities.jsp");

            //geting the response from the request url
            var response = (HttpWebResponse)httpRequest.GetResponse();

            //create a stream to hold the contents of the response (in this case it is the contents of the XML file
            var receiveStream = response.GetResponseStream();

            //creating XML document
            var mySourceDoc = new XmlDocument();

            //load the file from the stream
            mySourceDoc.Load(receiveStream);

            //close the stream
            receiveStream.Close();

            XmlNodeReader nodereader = new XmlNodeReader(mySourceDoc);
            //Display all the book titles.
            XmlNodeList elemList = mySourceDoc.GetElementsByTagName("code");
            for (int i = 0; i < elemList.Count; i++)
            {
                String Valor = elemList[i].InnerXml;
                if (lada.CompareTo(Valor) == 0)
                {
                    bandera = true;
                }
            }

            return bandera;
        }
        public String GetNumberFromList(String Lada)
        {

            //requesting the particular web page
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://www.agendize.com/web/templates/12512922/getnumbers.jsp?code=" + Lada);

            //geting the response from the request url
            var response = (HttpWebResponse)httpRequest.GetResponse();

            //create a stream to hold the contents of the response (in this case it is the contents of the XML file
            var receiveStream = response.GetResponseStream();

            //creating XML document
            var mySourceDoc = new XmlDocument();

            //load the file from the stream
            mySourceDoc.Load(receiveStream);

            //close the stream
            receiveStream.Close();

            XmlNodeReader nodereader = new XmlNodeReader(mySourceDoc);
            //Display all the book titles.
            XmlNodeList elemList = mySourceDoc.GetElementsByTagName("number");
            for (int i = 0; i < elemList.Count; i++)
            {

                String numero = null;
                numero = elemList[i].InnerXml;
                if (numero != null)
                    return numero;

            }
            return null;
        }
        public Boolean PostXml(String TrackingLine, Registro reg)
        {
            String xmlExt;
            String xmlEmail;
            String xmlSms;
            Boolean bandera = false;
            string url = "https://www.agendize.com/web/templates/12512922/setcalltracking.jsp";
            string xml = "<agendize>"
                        + "<action>create</action>"
                        + "<calltracking>" + TrackingLine + "</calltracking>"
                        + "<realnumber>" + reg.Telreal + "</realnumber>"
                        + "<misc1>" + reg.Customer_id + "</misc1>"
                        + "<misc2>" + reg.Bctline + "</misc2>"
                        + "<misc3>" + reg.Product_code + "</misc3>"
                        + "<misc4>" + reg.heading + "</misc4>"
                        + "<misc5>" + reg.Udac_code + "</misc5>"
                        + "<misc6>" + reg.Issue_year + "</misc6>"
                        + "<record>true</record>"
                        + "<record-audiocustom>http://graficos.seccionamarilla.com.mx/LocTrackLinesOK.mp3</record-audiocustom>"
                        + "<media-type>print</media-type>"
                        + "<name-media>PRINT</name-media>"
                        + "<publication-startdate>" + DateTime.Now.ToShortDateString() + "</publication-startdate>";

            if (reg.ext.Equals("")!=true)
            {
                xmlExt = "<extension>" + reg.ext + "</extension>";
                xml = xml + xmlExt;
            }

            if (reg.email.Equals("") != true)
            {
                xmlEmail = "<notification-email>true</notification-email>"
                            + "<email>" + reg.email + "</email>";
                xml = xml + xmlEmail;
            }

            if (reg.TelSms.Equals("") != true)
            {
                xmlSms = "<notification-sms>true</notification-sms>"
                                + "<sms>521" + reg.TelSms + "</sms>";
                xml = xml + xmlSms;
            }
            String xmlFinalize = "</agendize>";
            xml = xml + xmlFinalize;


            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "text/xml";
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)
            request.GetResponse())
            {
                var receiveStream = response.GetResponseStream();
                var mySourceDoc = new XmlDocument();

                //load the file from the stream
                mySourceDoc.Load(receiveStream);

                //close the stream
                receiveStream.Close();

                XmlNodeReader nodereader = new XmlNodeReader(mySourceDoc);
                //Display all the book titles.
                XmlNodeList elemList = mySourceDoc.GetElementsByTagName("result");
                for (int i = 0; i < elemList.Count; i++)
                {

                    String respuesta = null;
                    respuesta = elemList[i].InnerXml;

                    if (respuesta.CompareTo("200 OK") == 0)
                    {
                        bandera = true;
                    }

                }
            }
            return bandera;




        }
        public String GetkNumberReal(DataRow row)
        {
            String ad_id = row["PRD_IDENTIFICATION"].ToString();
            String Advertiser_id = row["CUSTOMER_ID"].ToString();
            Datos.Datos datos = new Datos.Datos();
            return datos.GetRealNumber(ad_id, Advertiser_id);

        }
        public Boolean ActualizaTel(DataRow row)
        {
            String TRANSACTION_ID = row["TRANSACTION_ID"].ToString();
            Datos.Datos datos = new Datos.Datos();
            return datos.Actualizareg(TRANSACTION_ID);
        }
        public Boolean ActualizaTelAlt(String Transaction_id)
        {
            String TRANSACTION_ID = Transaction_id;
            Datos.Datos datos = new Datos.Datos();
            return datos.Actualizareg(TRANSACTION_ID);
        }
        /* public Boolean ActualizaAcms(DataRow row, String TrLn)
         {
             String ad_id = row["PRD_IDENTIFICATION"].ToString();
             String Advertiser_id = row["CUSTOMER_ID"].ToString();
             Datos.Datos datos = new Datos.Datos();
             return datos.ActualizaAcmsContent(Advertiser_id, ad_id, TrLn);

         }*/
        public Boolean ActualizaAcmsAlter(String Ad_id, String Advertiser, String TrLn, String DirIssue, String PrdCode)
        {
            String ad_id = Ad_id;
            String Advertiser_id = Advertiser;
            String Dir_Issue = DirIssue;
            String Prd_Code = PrdCode;
            Datos.Datos datos = new Datos.Datos();
            return datos.ActualizaAcmsContent(Advertiser_id, ad_id, TrLn, Dir_Issue, Prd_Code);

        }
        public Boolean DelPostXml(String TrackingLine, Registro reg)
        {

            Boolean bandera = false;
            string url = "https://www.agendize.com/web/templates/12512922/setcalltracking.jsp";
            string xml = "<agendize>"
                        + "<action>delete</action>"
                        + "<calltracking>" + TrackingLine + "</calltracking>"
                        + "<realnumber>" + reg.Telreal + "</realnumber>"
                        + "<record>true</record>"
                        + "<media-type>website</media-type>"
                        + "<publication-startdate>" + DateTime.Now.ToShortDateString() + "</publication-startdate>"
                        + "</agendize>";

            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "text/xml";
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)
            request.GetResponse())
            {
                var receiveStream = response.GetResponseStream();
                var mySourceDoc = new XmlDocument();

                //load the file from the stream
                mySourceDoc.Load(receiveStream);

                //close the stream
                receiveStream.Close();

                XmlNodeReader nodereader = new XmlNodeReader(mySourceDoc);
                //Display all the book titles.
                XmlNodeList elemList = mySourceDoc.GetElementsByTagName("result");
                for (int i = 0; i < elemList.Count; i++)
                {

                    String respuesta = null;
                    respuesta = elemList[i].InnerXml;

                    if (respuesta.CompareTo("200 OK") == 0)
                    {
                        bandera = true;
                    }

                }
            }
            return bandera;

        }
        public DataTable GetEelements()
        {
            Datos.Datos datos = new Datos.Datos();
            return datos.GetPendingEelements();

        }
        public String GetActualTelVersion(String ad_id)
        {
            Datos.Datos datos = new Datos.Datos();
            return datos.GetActualTelVersion(ad_id);
        }

        /* public String GetPreviousTelVersion()
         {
             Datos.Datos datos = new Datos.Datos();
         }*/



        /**/
        public Boolean UpdatetaTrlineLog(String Transaction_id, String TrLn)
        {
            Datos.Datos datos = new Datos.Datos();
            return datos.UpdatetaTrlineLog(Transaction_id, TrLn);
        }
        public DataTable GetPendingT()
        {
            Datos.Datos datos = new Datos.Datos();
            return datos.GetPendingT();
        }
        public String GetRealLada(String Telefono)
        {
            String Lada = Telefono.Substring(0, 2);
            if (Lada == "33" || Lada == "81" || Lada == "55")
            {
                Lada = Telefono.Substring(0, 2);
            }
            else
            {
                Lada = Telefono.Substring(0, 3);
            }
            return Lada;
        }


        public void ProccessT()
        {
            DataTable TrLnElements = null;
            /*Variables de datos*/
            Registro registro = new Registro();
            String Prd_identificationCom;
            String Lada;
            Boolean ExisteCiudad = false;
            String TrLn;
            Boolean Asignado = false;
            String NumberTocsi;
            Boolean actualizado;
            Boolean UpdateCsi;



            try
            {
                /*Buscando elementos con estatus T*/
                TrLnElements = GetPendingT();

                if (TrLnElements.Rows.Count > 0)
                {

                    foreach (DataRow row in TrLnElements.Rows)
                    {

                        try //ABRAHAM: Le agregue este try para que ejecute todos los registros 
                        {
                            registro.Transaction_id = row["TRANSACTION_ID"].ToString();
                            registro.Prefix = row["PRD_ID_PREFIX"].ToString();
                            registro.Prd_identification = row["PRD_IDENTIFICATION"].ToString();
                            registro.Customer_id = row["CUSTOMER_ID"].ToString();
                            registro.Product_code = row["PRODUCT_CODE"].ToString();
                            registro.Issue_year = row["ISSUE_YEAR"].ToString();
                            registro.Udac_code = row["UDAC_CODE"].ToString();
                            registro.Bc_product_id = row["BC_PRODUCT_ID"].ToString();
                            registro.Bctline = row["BCTLINE"].ToString();
                            registro.Telreal = row["TELREAL"].ToString();
                            registro.TltrLine = row["TLTRLINE"].ToString();
                            registro.Status = row["STATUS"].ToString();
                            registro.Procesado = row["PROCESADO"].ToString();
                            registro.TelSms = row["TELSMS"].ToString();
                            registro.email = row["EMAIL"].ToString();
                            registro.ext = row["TELEXT"].ToString();
                            registro.heading = row["HEADING_NAME"].ToString();
                            Prd_identificationCom = row["PRD_IDENTIFICATION"].ToString();


                            /*Verificamos la existencia de la ciudad*/
                            Lada = GetRealLada(registro.Telreal);
                            if (GetExisteCiudadParaLada(Lada) == true)/*La ciudad existe*/
                            {
                                /*Obtenemos numero de Agendize para la ciudad*/
                                TrLn = GetTkNumber(Lada);
                                if (TrLn != null)/*Se ha regresado un telefono*/
                                {
                                    if (TrLn != "unavailable")
                                    {
                                        /*Se adiciona un 52 para la peticion a Agendize*/
                                        registro.Telreal = "52" + registro.Telreal;
                                        /*Se envia la peticion para enlazar los numeros*/
                                        Asignado = PostXml(TrLn, registro);
                                        if (Asignado == true)/*Se ha enlazado exitosamente*/
                                        {

                                            /*Se envia su insercion a IAM */
                                            actualizado = ActualizaAcmsAlter(Prd_identificationCom, registro.Customer_id, TrLn, registro.Issue_year, registro.Product_code);
                                            if (actualizado == true)/*se ha actualizado de manera adecuada*/
                                            {
                                                /*Se actualiza csi_transactions*/
                                                UpdateCsi = ActualizaTelAlt(registro.Transaction_id);
                                                if (UpdateCsi == true)/*Se ha actualizado a I correctamente*/
                                                {
                                                    /*Actualizamos TA_TRLINE_LOG*/
                                                    UpdatetaTrlineLog(registro.Transaction_id, TrLn);
                                                    /*Desliga*/
                                                    //DelPostXml(TrLn, registro);
                                                }
                                            }
                                            else
                                            {
                                                /*si la insercion a IAM fallo lo desligamos*/
                                                DelPostXml(TrLn, registro);
                                                PrintLog("fallo insercion a IAM para " + registro.Prd_identification);
                                            }

                                        }
                                        else
                                        {
                                            PrintLog("No se enlazo numero para " + registro.Prd_identification);
                                        }
                                    }
                                    else
                                    {
                                        PrintLog("unavailable para lada  " + Lada);

                                    }
                                }
                                else
                                {
                                    PrintLog("No se devolvio numero para  " + registro.Prd_identification);
                                }
                            }
                            else { PrintLog(registro.Prd_identification +" La lada no existe en Agendize " + Lada); }

                        }
                        catch (Exception e)
                        {
                            // ABRAHAM: Meter un LOG de Errores
                            PrintLog(e.Message.ToString());
                        }
                        /*limpiamos variables*/
                        registro = new Registro();
                        Lada = null;
                        ExisteCiudad = false;
                        TrLn = null;
                        Asignado = false;
                        NumberTocsi = null;
                        actualizado = false;
                        UpdateCsi = false;
                    }

                }
            }
            catch (Exception e)
            {
                //ABRAHAM: Meter un LOG de Errores
            }
        }
        public void PrintLog(String message)
        {
            StreamWriter sErr = null;
            try
            {
                string sFileError;
                sFileError = "C:\\ErrorTK_log.TXT";
                sErr = new StreamWriter(sFileError, true);
                sErr.WriteLine("Error(" + DateTime.Now.ToString() + "): " + message);
                sErr.WriteLine("----------------------------------------------------");
            }
            catch { }
            finally
            {
                sErr.Flush();
                sErr.Close();
            }
        }


        public void CorrigeTLINE()
        {

            DataTable TrLnElements = null;
            /*Variables de datos*/
            Registro registro = new Registro();
            String Prd_identificationCom;
            String Lada;
            Boolean ExisteCiudad = false;
            String TrLn;
            Boolean Asignado = false;
            String NumberTocsi;
            Boolean actualizado;
            Boolean UpdateCsi;
            
            /*Necesitamos los tlines ya procesados*/
            try
            {
                /*Buscando elementos procesados*/
                TrLnElements = GetTlineProcesados();
                if (TrLnElements.Rows.Count > 0)
                {

                    foreach (DataRow row in TrLnElements.Rows)//recorremos los elementos
                    {
                        registro.Transaction_id = row["TRANSACTION_ID"].ToString();
                        registro.Prefix = row["PRD_ID_PREFIX"].ToString();
                        registro.Prd_identification = row["PRD_IDENTIFICATION"].ToString();
                        registro.Customer_id = row["CUSTOMER_ID"].ToString();
                        registro.Product_code = row["PRODUCT_CODE"].ToString();
                        registro.Issue_year = row["ISSUE_YEAR"].ToString();
                        registro.Udac_code = row["UDAC_CODE"].ToString();
                        registro.Bc_product_id = row["BC_PRODUCT_ID"].ToString();
                        registro.Bctline = row["BCTLINE"].ToString();
                        registro.Telreal = "52"+row["TELREAL"].ToString();
                        registro.TltrLine = row["TLTRLINE"].ToString();
                        registro.Status = row["STATUS"].ToString();
                        registro.Procesado = row["PROCESADO"].ToString();
                        registro.TelSms = row["TELSMS"].ToString();
                        registro.email = row["EMAIL"].ToString();
                        registro.ext = row["TELEXT"].ToString();
                        registro.heading = row["HEADING_NAME"].ToString();
                        Prd_identificationCom = row["PRD_IDENTIFICATION"].ToString();


                        actualizado = PostXmlUpdate(registro);
                        if (actualizado != true)
                        {
                            PrintLog("actualizacion fallo para  " + registro.Bctline);
                        }
                        registro = new Registro();
                    }
                }
            }catch(Exception e){
            }

        }

        public DataTable GetTlineProcesados()
        {
            Datos.Datos datos = new Datos.Datos();
            return datos.GetTlineProcesados();
        }

        public bool PostXmlUpdate(Registro reg)
        {
            String xmlExt;
            String xmlEmail;
            String xmlSms;
            Boolean bandera = false;
            string url = "https://www.agendize.com/web/templates/12512922/setcalltracking.jsp";
            string xml = "<agendize>"
                        + "<action>edit</action>"
                        + "<calltracking>" + reg.TltrLine + "</calltracking>"
                        + "<realnumber>" + reg.Telreal + "</realnumber>"
                        + "<misc1>" + reg.Customer_id + "</misc1>"
                        + "<misc2>" + reg.Bctline + "</misc2>"
                        + "<misc3>" + reg.Product_code + "</misc3>"
                        + "<misc4>" + reg.heading + "</misc4>"
                        + "<misc5>" + reg.Udac_code + "</misc5>"
                        + "<misc6>" + reg.Issue_year + "</misc6>"
                        + "<record>true</record>"
                        + "<record-audiocustom>http://graficos.seccionamarilla.com.mx/LocTrackLinesOK.mp3</record-audiocustom>"
                        + "<media-type>print</media-type>"
                        + "<name-media>PRINT</name-media>"
                        + "<publication-startdate>" + DateTime.Now.ToShortDateString() + "</publication-startdate>";

            if (reg.ext.Equals("") != true)
            {
                xmlExt = "<extension>" + reg.ext + "</extension>";
                xml = xml + xmlExt;
            }

            if (reg.email.Equals("") != true)
            {
                xmlEmail = "<notification-email>true</notification-email>"
                            + "<email>" + reg.email + "</email>";
                xml = xml + xmlEmail;
            }

            if (reg.TelSms.Equals("") != true)
            {
                xmlSms = "<notification-sms>true</notification-sms>"
                                + "<sms>521" + reg.TelSms + "</sms>";
                xml = xml + xmlSms;
            }

            String xmlFinalize = "</agendize>";
            xml = xml + xmlFinalize;


            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "text/xml";
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)
            request.GetResponse())
            {
                var receiveStream = response.GetResponseStream();
                var mySourceDoc = new XmlDocument();

                //load the file from the stream
                mySourceDoc.Load(receiveStream);

                //close the stream
                receiveStream.Close();

                XmlNodeReader nodereader = new XmlNodeReader(mySourceDoc);
                //Display all the book titles.
                XmlNodeList elemList = mySourceDoc.GetElementsByTagName("result");
                for (int i = 0; i < elemList.Count; i++)
                {

                    String respuesta = null;
                    respuesta = elemList[i].InnerXml;

                    if (respuesta.CompareTo("200 OK") == 0)
                    {
                        bandera = true;
                    }

                }
            }
            return bandera;
        }

    }
}
