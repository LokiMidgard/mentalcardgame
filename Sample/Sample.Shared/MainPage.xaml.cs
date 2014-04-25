using MentalCardGame.RNG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Sample
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Frames navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            var rng = new RNG();
            var k0 = MentalCardGame.CardEngine.CreateGameKey(0, rng);
            var k1 = MentalCardGame.CardEngine.CreateGameKey(1, rng);
            var e0 = new MentalCardGame.CardEngine(k0, rng, 10, k0, k1);
            var e1 = new MentalCardGame.CardEngine(k1, rng, 10, k0, k1);

            // Test Ver und Entschlüsselung
            {
                var c = e0.CreateCard(3);
                var cc = c.MaskCard();
                var cc1 = e1.DeSerializeCard(e0.SerializeCard(cc));
                var t = cc1.UnmaskCard(cc.UncoverSecrets());

                System.Diagnostics.Debugger.Break();
            }

            // Test ProveCardFrom
            {
                var c0 = e0.CreateCard(10).MaskCard();
                var cc0 = c0.MaskCard();

                var xmlc = e0.SerializeCard(c0);
                var xmlcc = e0.SerializeCard(cc0);

                var c1 = e1.DeSerializeCard(xmlc);
                var cc1 = e1.DeSerializeCard(xmlcc);
                var eqc = c0 == c1;
                var eqcc = cc0 == cc1;

                var p0_1 = cc0.ProveCreatedFrom(15);
                var xmlp0_1 = e0.SerializeProveCreatedFromPhase1Output(p0_1);
                var p1_1 = e1.DeSerializeProveCreatedFromPhase1Output(xmlp0_1);

                var p1_2 = cc1.ProveCreatedFromPhase2(p1_1, c1);
                var xmlp1_2 = e1.SerializeProveCreatedFromPhase2Output(p1_2);
                var p0_2 = e0.DeSerializeProveCreatedFromPhase2Output(xmlp1_2);

                var p0_3 = cc0.ProveCreatedFromPhase3(p0_2, p0_1);
                var xmlp0_3 = e0.SerializeProveCreatedFromPhase3Output(p0_3);
                var p1_3 = e1.DeSerializeProveCreatedFromPhase3Output(xmlp0_3);

                var erg = cc1.ProveCreatedFromPhase4(p1_3, p1_2, p1_1);

                System.Diagnostics.Debugger.Break();
            }

            // Test Uncover Card
            {
                var c0 = e0.CreateCard(10).MaskCard();
                var cc0 = c0.MaskCard();

                var xmlc = e0.SerializeCard(c0);
                var xmlcc = e0.SerializeCard(cc0);

                var c1 = e1.DeSerializeCard(xmlc);
                var cc1 = e1.DeSerializeCard(xmlcc);
                var eqc = c0 == c1;
                var eqcc = cc0 == cc1;

                var u0_0 = cc0.UncoverSecrets();
                var xmlu0_0 = e0.SerializeProveUncoverdSecrets(u0_0);
                var u0_1 = e1.DeSerializeUncoverdSecrets(xmlu0_0);

                var u1_1 = cc1.UncoverSecrets();
                var xmlu1_1 = e1.SerializeProveUncoverdSecrets(u1_1);
                var u1_0 = e0.DeSerializeUncoverdSecrets(xmlu1_1);

                var t0 = cc0.UnmaskCard(u1_0);
                var t1 = cc1.UnmaskCard(u1_1, u0_1);

                System.Diagnostics.Debugger.Break();
            }

            // Stack Testen
            {
                var s0 = e0.CreateStack(e0.CreateCard(1), e0.CreateCard(2), e0.CreateCard(3), e0.CreateCard(4), e0.CreateCard(5));
                var ss0 = s0.Shuffle();

                var xmls = e0.SerializeStack(s0);
                var xmlss = e0.SerializeStack(ss0);

                var s1 = e1.DeSerializeStack(xmls);
                var ss1 = e1.DeSerializeStack(xmlss);
                var eqc = s0 == s1;
                var eqcc = ss0 == ss1;

                var p0_1 = ss0.ProveShuffle(15);
                var xmlp0_1 = e0.SerializeProveShufflePhase1Output(p0_1);
                var p1_1 = e1.DeSerializeProveShufflePhase1Output(xmlp0_1);

                var p1_2 = ss1.ProveShufflePhase2(s1, p1_1);
                var xmlp1_2 = e1.SerializeProveShufflePhase2Output(p1_2);
                var p0_2 = e0.DeSerializeProveShufflePhase2Output(xmlp1_2);

                var p0_3 = ss0.ProveShufflePhase3(p0_1, p0_2);
                var xmlp0_3 = e0.SerializeProveShufflePhase3Output(p0_3);
                var p1_3 = e1.DeSerializeProveShufflePhase3Output(xmlp0_3);

                var erg = ss1.ProveShufflePhase4(p1_1, p1_2, p1_3);

                System.Diagnostics.Debugger.Break();
            }
            App.Current.Exit();


            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Wird aufgerufen, wenn diese Seite in einem Rahmen angezeigt werden soll.
        /// </summary>
        /// <param name="e">Ereignisdaten, die beschreiben, wie diese Seite erreicht wurde.
        /// Dieser Parameter wird normalerweise zum Konfigurieren der Seite verwendet.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Seite vorbereiten, um sie hier anzuzeigen.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
    }
}
