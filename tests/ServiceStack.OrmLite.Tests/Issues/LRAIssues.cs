using System;
using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class AdhocJoinIssue : OrmLiteTestBase
    {
        [Test]
        public void Can_run_LRA_Query()
        {
            using (var db = OpenDbConnection())
            {
                CreateTables(db);

                var query = db.From<LRAAnalisi>()
                    .Join<LRAAnalisi, LRAContenitore>((ana, cont) => ana.ContenitoreId == cont.Id)
                    .Join<LRAContenitore, LRARichiesta>((cont, ric) => cont.RichiestaId == ric.Id)
                    .Join<LRARichiesta, LRAPaziente>((ric, paz) => ric.PazienteId == paz.Id)
                    .Select<LRAAnalisi, LRAContenitore, LRARichiesta, LRAPaziente>((ana, cont, ric, paz) =>
                        new
                        {
                            AnalisiId = ana.Id,
                            ContenitoreId = cont.Id,
                            RichiestaId = ric.Id,
                            DataAccettazioneRichiesta = ric.DataOraAccettazione,
                            DataCheckinContenitore = cont.DataOraPrimoCheckin,
                            DataEsecuzioneAnalisi = ana.DataOraEsecuzione,
                            DataPrelievoContenitore = cont.DataOraPrelievo,
                            DataValidazioneAnalisi = ana.DataOraValidazione,
                            sessoPaziente = paz.Sesso,
                            dataDiNascitaPaziente = paz.DataDiNascita,
                            etaPazienteRichiesta = ric.EtaPaziente,
                            dataOraAccettazioneRichiesta = ric.DataOraAccettazione,
                            unitaMisuraEtaPaziente = ric.UnitaDiMisuraEtaPaziente,
                            settimaneGravidanza = ric.SettimaneGravidanza,
                            repartoId = ric.RepartoId,
                            prioritaRichiesta = ric.PrioritaId,
                            laboratorioRichiedente = ric.LaboratorioRichiedenteId,
                            prioritaContenitore = cont.PrioritaId,
                            idLaboratorioEsecutoreCandidatoAnalisi = ana.LaboratorioEsecutoreCandidatoId
                        });
            }
        }

        [Test]
        public void Can_run_expression_using_captured_lambda_params()
        {
            using (var db = OpenDbConnection())
            {
                CreateTables(db);

                var idContenitore = 1;
                
                var q = db.From<LRAAnalisi>()
                    .Where(ana => ana.ContenitoreId == idContenitore)
                    .And(ana => !Sql.In(ana.Id, db.From<LRAContenitore>()
                            .Where(ris => ris.Id == ana.ContenitoreId)
                            .Select(ris => new { Id = ris.Id })
                        )
                    )
                    .Select(x => new { A = 1 });

                var sql = q.ToSelectStatement();
                sql.Print();
                Assert.That(sql, Is.Not.Null);
            }
        }

        private static void CreateTables(IDbConnection db)
        {
            db.DropTable<LRAAnalisi>();
            db.DropTable<LRAContenitore>();
            db.DropTable<LRARichiesta>();
            db.DropTable<LRAPaziente>();
            db.DropTable<LRDReparto>();
            db.DropTable<LRDPriorita>();
            db.DropTable<LRDLaboratorio>();
            
            db.CreateTable<LRDLaboratorio>();
            db.CreateTable<LRDPriorita>();
            db.CreateTable<LRDReparto>();
            db.CreateTable<LRAPaziente>();
            db.CreateTable<LRARichiesta>();
            db.CreateTable<LRAContenitore>();
            db.CreateTable<LRAAnalisi>();
        }
    }

    public class DBObject
    {        
    }

    public class SessiPaziente
    {
        public const int NonDichiarato = 0;
    }

    public class UnitaMisuraEtaPaziente
    {
        public const int Anni = 0;
    }
    
    [Alias("LRAANALISI")]
//    [CompositeIndex("AANALISIPADREID", "ACONTENITOREID", Unique = false, Name = "IDXLRAANALISI")]
//    [CompositeIndex("ACONTENITOREID", "DANALISIID", "AANALISIPADREID", "LIVELLOANALISI", Unique = true, Name = "IDXLRAANALISI2")]
    public class LRAAnalisi : IHasId<int>
    {
        [PrimaryKey]
        [AutoIncrement]        
        [Alias("IDAANALISI")]
        public int Id { get; set; }

        [Alias("ACONTENITOREID")]
        [References(typeof(LRAContenitore))]
        public int ContenitoreId { get; set; }

        [Reference]
        public LRAContenitore Contenitore { get; set; }

//        [Alias("DANALISIID")]
//        [References(typeof(LRDAnalisi))]
//        public int AnalisiId { get; set; }
//
//        [Reference]
//        public LRDAnalisi Analisi { get; set; }
//
//        [Alias("AANALISIPADREID")]
//        [References(typeof(LRAAnalisi))]
//        public int? AnalisiPadreId { get; set; }
//
//        [Reference]
//        public LRAAnalisi AnalisiPadre { get; set; }
//
//        [Default(0)]
//        [Alias("LIVELLOANALISI")]
//        public int LivelloAnalisi { get; set; }
//
//        [Alias("STATO")]
//        [Default((int) StatiAnalisi.InAttesa)]
//        public int Stato { get; set; }
//
//        [Alias("DTIPOCONVALIDAID")]
//        [References(typeof(LRDTipoConvalida))]
//        public int? TipoConvalidaId { get; set; }
//
//        [Reference]
//        public LRDTipoConvalida TipoConvalida { get; set; }
//
        [Alias("DATAORAVALIDAZIONE")]
        public DateTime? DataOraValidazione { get; set; }

//        [Alias("DOPERATOREVALIDAZIONEID")]
//        [References(typeof(LRDOperatore))]
//        public int? OperatoreValidazioneId { get; set; }
//
//        [Reference]
//        public LRDOperatore OperatoreValidazione { get; set; }
//
        [Alias("DLABORATORIOCANDIDATOID")]
        [References(typeof(LRDLaboratorio))]
        public int? LaboratorioEsecutoreCandidatoId { get; set; }

//        [Reference]
//        public LRDLaboratorio Laboratorio { get; set; }
//
        [Alias("DATAORAESECUZIONE")]
        public DateTime? DataOraEsecuzione { get; set; }
        
//        [Alias("COMMENTO")]
//        [StringLength(StringLengthAttribute.MaxText)]
//        public string Commento { get; set; }
//
//        [Alias("VALIDAZIONEAUTOMATICA")]
//        [Default((int) ModalitaValidazione.ValidatoManualmente)]
//        public int? ValidazioneAutomatica { get; set; }
//
//        [Alias("DATAORACHECKIN")]
//        public DateTime? DataOraCheckin { get; set; }
//
//        [Alias("DATAORAACCETTAZIONE")]
//        public DateTime DataOraAccettazione { get; set; }
//
//        [Alias("DATAORAPRELIEVO")]
//        public DateTime DataOraPrelievo { get; set; }
    }
    
    [Alias("LRACONTENITORI")]
//    [CompositeIndex("BARCODE", Unique = false, Name = "NCI_BARCODE")]
//    [CompositeIndex("ARICHIESTAID", Unique = false, Name = "NCI_ARICHIESTAID")]
    public class LRAContenitore : DBObject, IHasId<int>
    {
//        private string _barcode;
//
        [PrimaryKey]
        [AutoIncrement]
        [Alias("IDACONTENITORE")]
        public int Id { get; set; }

//        [Required]
//        [Alias("BARCODE")]
//        [StringLength(AppDBModelFieldLength.C_BARCODE)]
//        public string Barcode
//        {
//            get
//            {
//                return _barcode;
//            }
//            set
//            {
//                _barcode = value.Fix(AppDBModelFieldLength.C_BARCODE);
//            }
//        }

        [Alias("ARICHIESTAID")]
        [References(typeof(LRARichiesta))]
        public int RichiestaId { get; set; }
        
//        [Reference]
//        public LRARichiesta Richiesta { get; set; }
//
//        [Alias("DCONTENITOREID")]
//        [References(typeof(LRDContenitore))]
//        public int ContenitoreId { get; set; }
//
//        [Reference]
//        public LRDContenitore Contenitore { get; set; }
//
        [Alias("DPRIORITAID")]
        [References(typeof(LRDPriorita))]
        public int PrioritaId { get; set; }

//        [Reference]
//        public LRDPriorita Priorita { get; set; }
//
        [Alias("DATAORAPRELIEVO")]
        public DateTime? DataOraPrelievo { get; set; }

        [Alias("DATAORAPRIMOCHECKIN")]
        public DateTime? DataOraPrimoCheckin { get; set; }

//        [References(typeof(LRDDevice))]
//        [Alias("DDEVICEIDPRIMOCHECKIN")]        
//        public int? DeviceIdPrimoCheckIn { get; set; }
//
//        [Reference]
//        public LRDDevice DevicePrimoCheckIn { get; set; }
//
//        [Alias("STATO")]
//        [Default((int) StatiContenitore.NonPervenuto)]        
//        public int Stato { get; set; }
//
//        [Alias("DTIPOCONVALIDAID")]
//        [References(typeof(LRDTipoConvalida))]
//        public int? TipoConvalidaId { get; set; }
//
//        [Reference]
//        public LRDTipoConvalida TipoConvalida { get; set; }
//
//        [Alias("DATAORAVALIDAZIONE")]
//        public DateTime? DataOraValidazione { get; set; }
//
//        [Alias("DATAORAESECUZIONE")]
//        public DateTime? DataOraEsecuzione { get; set; }
//
//        [Alias("DATAORAULTIMARIPETIZIONE")]
//        public DateTime? DataOraUltimaRipetizione { get; set; }
//
//        [Default((int) SiNo.No)]
//        [Alias("RIPETIZIONECITRATO")]
//        public int RipetizioneCitrato { get; set; }
//
//        [Alias("DATAORARICHIESTAVETRINOAUTO")]        
//        public DateTime? DataOraRichiestaVetrinoAutomatico { get; set; }
//
//        [Alias("DATAORARICHIESTAVETRINOMAN")]
//        public DateTime? DataOraRichiestaVetrinoManuale { get; set; }
//
//        [Alias("COMMENTO")]
//        [StringLength(StringLengthAttribute.MaxText)]
//        public string Commento { get; set; }
//
//        [Alias("COMMENTOINTERNO")]
//        [StringLength(StringLengthAttribute.MaxText)]
//        public string CommentoInterno { get; set; }
//        
//        [Alias("COMMENTOREFERTO")]
//        [StringLength(StringLengthAttribute.MaxText)]
//        public string CommentoReferto { get; set; }
//
//        [Alias("VALIDAZIONEAUTOMATICA")]
//        [Default((int) ModalitaValidazione.ValidatoManualmente)]  
//        public int? ValidazioneAutomatica { get; set; }
    }

    [Alias("LRARICHIESTE")]
//    [CompositeIndex("NUMERORICHIESTA", "DATAORAACCETTAZIONE", Unique = true, Name = "IDXLRARICHIESTE")]
    public class LRARichiesta : DBObject, IHasId<int>
    {
//        private string _numero_richiesta;
//        private string _numero_ricovero;
//
        [PrimaryKey]
        [AutoIncrement]
        [Alias("IDARICHIESTA")]                
        public int Id { get; set; }
        
        [Alias("APAZIENTEID")]
        [ForeignKey(typeof(LRAPaziente))]
        public int PazienteId { get; set; }

//        [Reference]
//        public LRAPaziente Paziente { get; set; }
//             
//        [Required]
//        [Alias("NUMERORICHIESTA")]
//        [StringLength(AppDBModelFieldLength.C_NUMERO_RICHIESTA)]
//        public string NumeroRichiesta
//        {
//            get
//            {
//                return _numero_richiesta;
//            }
//            set
//            {
//                _numero_richiesta = value.Fix(AppDBModelFieldLength.C_NUMERO_RICHIESTA);
//            }
//        }
//              
//        [Index(Unique = false)]
//        [Alias("NUMERORICOVERO")]
//        [StringLength(AppDBModelFieldLength.C_NUMERO_RICOVERO)]
//        public string NumeroRicovero
//        {
//            get
//            {
//                return _numero_ricovero;
//            }
//            set
//            {
//                _numero_ricovero = value.Fix(AppDBModelFieldLength.C_NUMERO_RICOVERO);
//            }
//        }
//
        [Required]
        [Index(Unique = false)]
        [Alias("DATAORAACCETTAZIONE")]                        
        public DateTime DataOraAccettazione { get; set; }

//        [Required]
//        [Index(Unique = false)]
//        [Alias("DATAORAPRELIEVO")]        
//        public DateTime DataOraPrelievo { get; set; }
        
        [Alias("ETAPAZIENTE")]
        public int? EtaPaziente { get; set; }

        [Alias("UNITADIMISURAETAPAZIENTE")]
        [Default((int) UnitaMisuraEtaPaziente.Anni)]
        public int UnitaDiMisuraEtaPaziente { get; set; }

        [Alias("SETTIMANEGRAVIDANZA")]               
        public int? SettimaneGravidanza { get; set; }
        
        [Alias("DREPARTOID")]
        [References(typeof(LRDReparto))]
        public int? RepartoId { get; set; }
        
//        [Reference]
//        public LRDReparto Reparto { get; set; }
//        
//        [Alias("DDEVICEID")]
//        [References(typeof(LRDDevice))]
//        public int? DeviceId { get; set; }
//
//        [Reference]
//        public LRDDevice Device { get; set; }
//
        [Required]
        [Alias("DPRIORITAID")]
        [References(typeof(LRDPriorita))]        
        public int PrioritaId { get; set; }
//
//        [Reference]
//        public LRDPriorita Priorita { get; set; }
//                               
        [Required]
        [References(typeof(LRDLaboratorio))]
        [Alias("DLABORATORIORICHIEDENTEID")]        
        public int? LaboratorioRichiedenteId { get; set; }
        
//        [Reference]
//        public LRDLaboratorio LaboratorioRichiedente { get; set; }
//
//        [Alias("STATO")]
//        [Default((int) StatiRichiesta.Inserita)]
//        public int Stato { get; set; }
//
//        [Alias("DTIPOCONVALIDAID")]
//        [References(typeof(LRDTipoConvalida))]
//        public int? TipoConvalidaId { get; set; }
//
//        [Reference]
//        public LRDTipoConvalida TipoConvalida { get; set; }
//
//        [Alias("DATAORAVALIDAZIONE")]        
//        public DateTime? DataOraValidazione { get; set; }
//
//        [Alias("DATAORAESECUZIONE")]
//        public DateTime? DataOraEsecuzione { get; set; }
//
//        [Alias("COMMENTO")]
//        [StringLength(StringLengthAttribute.MaxText)]
//        public string Commento { get; set; }
//        
//        [Required]
//        [Alias("ARCHIVIATA")]
//        [Default((int) SiNo.No)] // default 0: in corso
//        public int Archiviata { get; set; }
//
//        [Alias("VALIDAZIONEAUTOMATICA")]                
//        [Default((int) ModalitaValidazione.ValidatoManualmente)]
//        public int? ValidazioneAutomatica { get; set; }
    }
    
    [Alias("LRAPAZIENTI")]
//    [CompositeIndex("COGNOME", "NOME", "DATADINASCITA", Unique = false, Name = "IDXLRAPAZIENTI")]
    public class LRAPaziente : DBObject, IHasId<int>
    {
//        private string _cognome;
//        private string _nome;
//        private string _tessera_sanitaria;
//        private string _codice_fiscale;
//        private string _cap;
//        private string _localita;
//        private string _indirizzo;
//        private string _provincia;
//        private string _telefono;
//        private string _cellulare;
//        private string _fax;
//        private string _email;
//
        [PrimaryKey]
        [AutoIncrement]
        [Alias("IDAPAZIENTE")]               
        public int Id { get; set; }
                          
//        [Required]
//        [Alias("PID")]
//        [Index(Unique = true)]
//        [StringLength(AppDBModelFieldLength.C_PID)]        
//        public string PID { get; set; }
//
//        [Index(Unique = false)]
//        [Alias("CODICEFISCALE")]               
//        [StringLength(AppDBModelFieldLength.C_CODICE_FISCALE)]
//        public string CodiceFiscale
//        {
//            get
//            {
//                return _codice_fiscale;
//            }
//            set
//            {
//                _codice_fiscale = value.Fix(AppDBModelFieldLength.C_CODICE_FISCALE);
//            }
//        }
//
//        [Index(Unique = false)]
//        [Alias("TESSERASANITARIA")]
//        [StringLength(AppDBModelFieldLength.C_TESSERA_SANITARIA)]        
//        public string TesseraSanitaria
//        {
//            get
//            {
//                return _tessera_sanitaria;
//            }
//            set
//            {
//                _tessera_sanitaria = value.Fix(AppDBModelFieldLength.C_TESSERA_SANITARIA);
//            }
//        }
//        
//        [Required]
//        [Alias("COGNOME")]
//        [StringLength(AppDBModelFieldLength.C_COGNOME)]
//        public string Cognome
//        {
//            get
//            {
//                return _cognome;
//            }
//            set
//            {
//                _cognome = value.Fix(AppDBModelFieldLength.C_COGNOME);
//            }
//        }
//        
//        [Required]
//        [Alias("NOME")]
//        [StringLength(AppDBModelFieldLength.C_NOME)]
//        public string Nome
//        {
//            get
//            {
//                return _nome;
//            }
//            set
//            {
//                _nome = value.Fix(AppDBModelFieldLength.C_NOME);
//            }
//        }
               
        [Alias("SESSO")]
        [Default((int) SessiPaziente.NonDichiarato)]
        public int Sesso { get; set; }

        [Alias("DATADINASCITA")]
        public DateTime? DataDiNascita { get; set; }
        
//        [Alias("INDIRIZZO")]
//        [StringLength(AppDBModelFieldLength.C_INDIRIZZO)]
//        public string Indirizzo
//        {
//            get
//            {
//                return _indirizzo;
//            }
//            set
//            {
//                _indirizzo = value.Fix(AppDBModelFieldLength.C_INDIRIZZO);
//            }
//        }
//        
//        [Alias("CAP")]
//        [StringLength(AppDBModelFieldLength.C_CAP)]
//        public string CAP
//        {
//            get
//            {
//                return _cap;
//            }
//            set
//            {
//                _cap = value.Fix(AppDBModelFieldLength.C_CAP);
//            }
//        }
//        
//        [Alias("LOCALITA")]
//        [StringLength(AppDBModelFieldLength.C_LOCALITA)]
//        public string Localita
//        {
//            get
//            {
//                return _localita;
//            }
//            set
//            {
//                _localita = value.Fix(AppDBModelFieldLength.C_LOCALITA);
//            }
//        }
//        
//        [Alias("PROVINCIA")]
//        [StringLength(AppDBModelFieldLength.C_PROVINCIA)]
//        public string Provincia
//        {
//            get
//            {
//                return _provincia;
//            }
//            set
//            {
//                _provincia = value.Fix(AppDBModelFieldLength.C_PROVINCIA);
//            }
//        }
//        
//        [Alias("TELEFONO")]
//        [StringLength(AppDBModelFieldLength.C_TELEFONO)]
//        public string Telefono
//        {
//            get
//            {
//                return _telefono;
//            }
//            set
//            {
//                _telefono = value.Fix(AppDBModelFieldLength.C_TELEFONO);
//            }
//        }
//        
//        [Alias("CELLULARE")]
//        [StringLength(AppDBModelFieldLength.C_CELLULARE)]
//        public string Cellulare
//        {
//            get
//            {
//                return _cellulare;
//            }
//            set
//            {
//                _cellulare = value.Fix(AppDBModelFieldLength.C_CELLULARE);
//            }
//        }
//        
//        [Alias("FAX")]
//        [StringLength(AppDBModelFieldLength.C_FAX)]
//        public string Fax
//        {
//            get
//            {
//                return _fax;
//            }
//            set
//            {
//                _fax = value.Fix(AppDBModelFieldLength.C_FAX);
//            }
//        }
//        
//        [Alias("EMAIL")]
//        [StringLength(AppDBModelFieldLength.C_EMAIL)]
//        public string Email
//        {
//            get
//            {
//                return _email;
//            }
//            set
//            {
//                _email = value.Fix(AppDBModelFieldLength.C_EMAIL);
//            }
//        }
//
//        [Alias("COMMENTO")]               
//        [StringLength(StringLengthAttribute.MaxText)]
//        public string Commento { get; set; }
//               
//        [Alias("DPATOLOGIAID")]
//        [References(typeof(LRDPatologia))]
//        public int? PatologiaId { get; set; }
    }

    public class LRDReparto
    {
        public int Id { get; set; }
    }

    public class LRDPriorita
    {
        public int Id { get; set; }
    }

    public class LRDLaboratorio
    {
        public int Id { get; set; }
    }
}