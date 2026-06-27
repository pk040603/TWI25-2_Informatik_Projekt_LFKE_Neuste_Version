Module Module1

#Region "KonstantenUndGlobals"

    ' Konstanten: Tastatur 

    Const NO_KEY = 0
    Const CURSOR_LEFT = 1
    Const CURSOR_RIGHT = 2
    Const UNKNOWN_KEY = 99


    ' Konstanten: Spielfeld

    Const SPALTE_MAX = 79
    Const ZEILE_MAX = 24
    Const BEWEGUNG_SPIELFIGUR = 10
    Const KOLLISIONS_ZEILE = ZEILE_MAX - 2


    ' Leitplanken / Strassenbreite

    Const LEITPLANKE_ZEICHEN As Char = "|"c

    Const STRASSE_BREITE_START As Double = 42.0
    Const STRASSE_BREITE_MIN As Double = 8.0
    Const STRASSE_BREITE_MAX As Double = 46.0
    Const STRASSE_MITTE_STANDARD As Double = SPALTE_MAX / 2


    ' --- Streckengenerator ---

    Const SEG_LAENGE_MIN = 10
    Const SEG_LAENGE_MAX = 30

    Const BREITE_SCHMAL As Double = 14.0
    Const BREITE_MITTEL As Double = 28.0
    Const BREITE_BREIT As Double = 42.0

    Const MITTE_WEIT_LINKS As Double = -14.0
    Const MITTE_LINKS As Double = -8.0
    Const MITTE_MITTE As Double = 0.0
    Const MITTE_RECHTS As Double = 8.0
    Const MITTE_WEIT_RECHTS As Double = 14.0

    Const GLAETTUNG As Double = 0.09


    ' CCM (Geschwindigkeit)

    Const CCM_50_WARTEZEIT = 300
    Const CCM_100_WARTEZEIT = 200
    Const CCM_150_WARTEZEIT = 120


    ' Items auf der Strecke

    Const ITEM_LINKS As Char = "["c
    Const ITEM_MITTE As Char = "?"c
    Const ITEM_RECHTS As Char = "]"c
    Const ITEM_CHANCE = 12

    Const ITEM_TYP_LEBEN = 0
    Const ITEM_TYP_SCHILD = 1
    Const ITEM_TYP_ULTIMATE = 2
    Const ITEM_TYP_SCHUSS = 3


    ' Wie viele Schuss eine eingesammelte Schuss-Box gibt

    Const SCHUSS_PRO_ITEM = 3


    ' Dauer eingesammelter Schilde (in Sekunden)

    Const SCHILD_DAUER_SEKUNDEN = 5


    ' Hindernisse + Schwierigkeit

    Const HINDERNIS_ZEICHEN As Char = "█"c
    Const HINDERNIS_CHANCE_START = 6
    Const HINDERNIS_CHANCE_MIN = 3
    Const SCHWIERIGKEIT_INTERVALL = 100


    ' Boost-Feld (2 Zeichen: ">>")

    Const BOOST_ZEICHEN As Char = ">"c


    ' Chance, dass in einer Zeile ein Boost-Feld erscheint (kleiner = häufiger)

    Const BOOST_CHANCE = 20


    ' Wie lange der Boost wirkt (in Sekunden)

    Const BOOST_DAUER_SEKUNDEN = 2


    ' Faktor, um den das Spiel waehrend des Boosts schneller laeuft

    Const BOOST_FAKTOR = 2


    ' Streckenmarkierungen (Startlinie, Meilenstein)
    ' Startlinie = schwarz-weiss kariert

    Const STARTLINIE_ZEICHEN As Char = "="c


    ' Meilenstein-Linie alle 100 m

    Const MEILENSTEIN_ZEICHEN As Char = "-"c
    Const MEILENSTEIN_INTERVALL = 100


    ' Streckentypen

    Const STRECKE_EIS = 1
    Const STRECKE_WUESTE = 2
    Const STRECKE_AUTOBAHN = 3

    ' Ab dieser Meterzahl wird Gold-Fahrzeug freigeschaltet
    Const GOLD_UNLOCK_METER = 400

    ' Ultimates (Ticks, Slowmo, Goldräumung)
    ' Buggy = Unverwundbarkeit in Ticks

    Const ULT_BUGGY_TICKS = 25


    ' Standard (Vollbremsung): Dauer von Zeitlupe + Unverwundbarkeit (in Ticks)

    Const ULT_VOLLBREMSUNG_TICKS = 40


    ' Faktor, um den das Spiel während der Zeitlupe langsamer läuuft

    Const ULT_SLOWMO_FAKTOR = 2


    ' Gold (Goldräumung): so viele Meter lang spawnen keine neuen Hindernisse
    ' (ca. 3 Sekunden im Spiel)

    Const GOLD_RAEUMUNG_METER = 15


    Const ACH_ANZAHL = 7

    ' Globale Spielvariablen

    Dim g_ccm As Integer = 100
    Dim g_strecke As Integer = STRECKE_AUTOBAHN
    Dim g_fahrzeug As Integer = 1
    Dim g_goldFreigeschaltet As Boolean = False
    Dim g_itemMeldung As String = ""
    Dim g_itemMeldungTicks As Integer = 0

    Dim g_musikPlayer As New System.Media.SoundPlayer()


    ' Optionen

    Dim g_musikAn As Boolean = True
    Dim g_startLeben As Integer = 5


    ' Statistik (wird nach jeder Runde aktualisiert)

    Dim g_statSpiele As Integer = 0
    Dim g_statGesamtMeter As Integer = 0
    Dim g_statBesteMeter As Integer = 0
    Dim g_statItems As Integer = 0
    Dim g_statHindernisse As Integer = 0
    Dim g_statHindernisZerstoert As Integer = 0
    Dim g_streckeGefahren(3) As Boolean


    Dim g_achFreigeschaltet(ACH_ANZAHL - 1) As Boolean
    Dim g_neuAchievements As String = ""


    ' Highscore-Tabelle (max. 5 Einträge)

    Structure HighscoreEintrag
        Dim name As String
        Dim meter As Integer
    End Structure
    Dim g_highscores(4) As HighscoreEintrag
    Dim g_highscoreAnzahl As Integer = 0

#End Region

#Region "AudioUndHilfen"


    ' Tastatur

    Function Tastatur_Abfrage() As Integer
        If Not Console.KeyAvailable Then Return NO_KEY
        Dim cki As ConsoleKeyInfo = Console.ReadKey(True)
        If cki.Key = ConsoleKey.LeftArrow Then Return CURSOR_LEFT
        If cki.Key = ConsoleKey.RightArrow Then Return CURSOR_RIGHT
        Return UNKNOWN_KEY
    End Function


    ' --- Musik ---
    ' Startet nur wenn Musik in den Optionen eingeschaltet ist.

    Sub Musik_Starten(ByVal dateiname As String)
        If Not g_musikAn Then
            Musik_Stoppen()
            Return
        End If
        Try
            g_musikPlayer.Stop()
            g_musikPlayer.SoundLocation = dateiname
            g_musikPlayer.PlayLooping()
        Catch
        End Try
    End Sub

    Sub Musik_Stoppen()
        Try
            g_musikPlayer.Stop()
        Catch
        End Try
    End Sub

    Sub Sound_Abspielen(ByVal dateiname As String)
        If Not g_musikAn Then Return
        ' Kurzer Sound in eigenem Thread
        Dim t As New Threading.Thread(Sub()
                                          Try
                                              Dim player As New System.Media.SoundPlayer(dateiname)
                                              player.PlaySync()
                                          Catch
                                          End Try
                                      End Sub)
        t.IsBackground = True
        t.Start()
    End Sub

    Sub Sound_Ampel_Countdown()
        Sound_Abspielen("Ampel-Countdown.wav")
    End Sub

    Sub Sound_GameOver()
        Sound_Abspielen("Game-Over.wav")
    End Sub


    ' Text zentriert auf dem Bildschirm

    Sub Zentriert_Schreiben(ByVal text As String, ByVal zeile As Integer)
        Dim spalte As Integer = (SPALTE_MAX \ 2) - (text.Length \ 2)
        If spalte < 0 Then spalte = 0
        Console.SetCursorPosition(spalte, zeile)
        Console.Write(text)
    End Sub


    ' Startaufstellung + Ampel vor dem Rennen
    ' Ki-Entwurf - von uns integriert sowie spezfisch angepasst für das Spiel.

    Function Startaufstellung_Anzeigen() As Integer
        Randomize()
        Dim spielerPlatz As Integer = CInt(Math.Floor(VBMath.Rnd() * 3)) + 1

        Dim gegner1Symbol As String = "???"
        Dim gegner2Symbol As String = "???"

        Select Case spielerPlatz
            Case 1
                gegner1Symbol = " /\ " : gegner2Symbol = "[BB]"
            Case 2
                gegner1Symbol = "/##\" : gegner2Symbol = "=$=$"
            Case 3
                gegner1Symbol = "\##/" : gegner2Symbol = " /\ "
        End Select

        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()

        Console.ForegroundColor = ConsoleColor.Yellow
        Zentriert_Schreiben("*" & New String("=", 35) & "*", 1)
        Zentriert_Schreiben("S T A R T A U F S T E L L U N G", 2)
        Zentriert_Schreiben("*" & New String("=", 35) & "*", 3)

        Console.ForegroundColor = ConsoleColor.Yellow
        Zentriert_Schreiben("P L A T Z   1", 5)
        Console.ForegroundColor = If(spielerPlatz = 1, Fahrzeug_Farbe(g_fahrzeug), ConsoleColor.DarkGray)
        Zentriert_Schreiben(If(spielerPlatz = 1,
                               Fahrzeug_Zeile1(g_fahrzeug) & "  <-- DU (Platz 1)",
                               gegner1Symbol & "  Gegner"), 6)

        Console.ForegroundColor = ConsoleColor.Yellow
        Zentriert_Schreiben("P L A T Z   2", 8)
        Console.ForegroundColor = If(spielerPlatz = 2, Fahrzeug_Farbe(g_fahrzeug), ConsoleColor.DarkGray)
        Zentriert_Schreiben(If(spielerPlatz = 2,
                               Fahrzeug_Zeile1(g_fahrzeug) & "  <-- DU (Platz 2)",
                               If(spielerPlatz = 1, gegner2Symbol & "  Gegner",
                                                    gegner1Symbol & "  Gegner")), 9)

        Console.ForegroundColor = ConsoleColor.Yellow
        Zentriert_Schreiben("P L A T Z   3", 11)
        Console.ForegroundColor = If(spielerPlatz = 3, Fahrzeug_Farbe(g_fahrzeug), ConsoleColor.DarkGray)
        Zentriert_Schreiben(If(spielerPlatz = 3,
                               Fahrzeug_Zeile1(g_fahrzeug) & "  <-- DU (Platz 3)",
                               gegner2Symbol & "  Gegner"), 12)

        Console.ForegroundColor = ConsoleColor.DarkGray
        Zentriert_Schreiben(New String("-", 37), 14)
        Console.ForegroundColor = ConsoleColor.White
        Zentriert_Schreiben("Du startest auf Platz " & spielerPlatz & " – viel Erfolg!", 15)

        Threading.Thread.Sleep(2000)
        Musik_Stoppen()

        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()

        Console.ForegroundColor = ConsoleColor.Yellow
        Zentriert_Schreiben("*" & New String("=", 35) & "*", 1)
        Zentriert_Schreiben("S T A R T A U F S T E L L U N G", 2)
        Zentriert_Schreiben("*" & New String("=", 35) & "*", 3)

        Console.ForegroundColor = ConsoleColor.DarkGray
        Zentriert_Schreiben("Du startest auf Platz " & spielerPlatz & " – viel Erfolg!", 5)

        Console.ForegroundColor = ConsoleColor.DarkGray
        Zentriert_Schreiben("+-------+", 8)
        Zentriert_Schreiben("|  ( )  |", 9)
        Zentriert_Schreiben("|  ( )  |", 11)
        Zentriert_Schreiben("|  ( )  |", 13)
        Zentriert_Schreiben("+-------+", 15)

        Dim ampelSpalte As Integer = (SPALTE_MAX \ 2) - 4

        Sound_Ampel_Countdown()

        Console.ForegroundColor = ConsoleColor.Red
        Console.SetCursorPosition(ampelSpalte, 9) : Console.Write("|  (█)  |")
        Console.ForegroundColor = ConsoleColor.DarkGray
        Console.SetCursorPosition(ampelSpalte, 11) : Console.Write("|  ( )  |")
        Console.SetCursorPosition(ampelSpalte, 13) : Console.Write("|  ( )  |")
        Console.ForegroundColor = ConsoleColor.Red
        Zentriert_Schreiben("  3  ", 17)
        Console.ForegroundColor = ConsoleColor.DarkGray
        Zentriert_Schreiben("     ", 18)
        Zentriert_Schreiben("     ", 19)
        Threading.Thread.Sleep(1000)

        Console.ForegroundColor = ConsoleColor.DarkGray
        Console.SetCursorPosition(ampelSpalte, 9) : Console.Write("|  ( )  |")
        Console.ForegroundColor = ConsoleColor.Yellow
        Console.SetCursorPosition(ampelSpalte, 11) : Console.Write("|  (█)  |")
        Console.ForegroundColor = ConsoleColor.DarkGray
        Console.SetCursorPosition(ampelSpalte, 13) : Console.Write("|  ( )  |")
        Console.ForegroundColor = ConsoleColor.DarkGray
        Zentriert_Schreiben("     ", 17)
        Console.ForegroundColor = ConsoleColor.Yellow
        Zentriert_Schreiben("  2  ", 18)
        Console.ForegroundColor = ConsoleColor.DarkGray
        Zentriert_Schreiben("     ", 19)
        Threading.Thread.Sleep(1000)

        Console.ForegroundColor = ConsoleColor.DarkGray
        Console.SetCursorPosition(ampelSpalte, 9) : Console.Write("|  ( )  |")
        Console.SetCursorPosition(ampelSpalte, 11) : Console.Write("|  ( )  |")
        Console.ForegroundColor = ConsoleColor.Green
        Console.SetCursorPosition(ampelSpalte, 13) : Console.Write("|  (█)  |")
        Console.ForegroundColor = ConsoleColor.DarkGray
        Zentriert_Schreiben("     ", 17)
        Zentriert_Schreiben("     ", 18)
        Console.ForegroundColor = ConsoleColor.Green
        Zentriert_Schreiben("  1  ", 19)
        Threading.Thread.Sleep(1000)

        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Green
        Zentriert_Schreiben("██╗      ██████╗  ███████╗ ██╗", 7)
        Zentriert_Schreiben("██║     ██╔═══██╗ ██╔════╝ ██║", 8)
        Zentriert_Schreiben("██║     ██║   ██║ ███████╗ ██║", 9)
        Zentriert_Schreiben("██║     ██║   ██║ ╚════██║ ╚═╝", 10)
        Zentriert_Schreiben("███████╗╚██████╔╝ ███████║ ██╗", 11)
        Zentriert_Schreiben("╚══════╝ ╚═════╝  ╚══════╝ ╚═╝", 12)
        Console.ForegroundColor = ConsoleColor.Yellow
        Zentriert_Schreiben("* * * * * * * * * * * * * * *", 14)
        Threading.Thread.Sleep(800)
        Console.Clear()

        Return spielerPlatz
    End Function

#End Region

#Region "StreckeUndFahrzeug"

    ' Strecken-Hilfsfunktionen

    Function Strecke_Name(ByVal id As Integer) As String
        Select Case id
            Case STRECKE_EIS : Return "Eisstrecke"
            Case STRECKE_WUESTE : Return "Wueste    "
            Case STRECKE_AUTOBAHN : Return "Autobahn  "
            Case Else : Return "Unbekannt "
        End Select
    End Function

    ' Konsolenfarben je Strecke

    Sub Strecke_Farben_Setzen(ByVal id As Integer)
        Select Case id
            Case STRECKE_EIS
                Console.BackgroundColor = ConsoleColor.DarkCyan
                Console.ForegroundColor = ConsoleColor.White
            Case STRECKE_WUESTE
                Console.BackgroundColor = ConsoleColor.DarkYellow
                Console.ForegroundColor = ConsoleColor.Black
            Case STRECKE_AUTOBAHN
                Console.BackgroundColor = ConsoleColor.DarkGray
                Console.ForegroundColor = ConsoleColor.White
        End Select
    End Sub

    ' Leitplanken: wechselnd weiss/rot
    ' Ki-Entwurf - von uns integriert sowie spezfisch angepasst für das Spiel.

    Sub Curb_Farbe_Setzen(ByVal curbPhase As Integer)
        If curbPhase Mod 2 = 0 Then
            Console.BackgroundColor = ConsoleColor.White
            Console.ForegroundColor = ConsoleColor.White
        Else
            Console.BackgroundColor = ConsoleColor.Red
            Console.ForegroundColor = ConsoleColor.Red
        End If
    End Sub

    Sub Strecke_Einleitung(ByVal id As Integer)
        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()

        Dim nameAnzeige As String = ""
        Dim rahmen As String = ""

        Select Case id
            Case STRECKE_EIS
                Console.ForegroundColor = ConsoleColor.Cyan
                nameAnzeige = "~~~  EISSTRECKE  ~~~"
                rahmen = "~~~~~~~~~~~~~~~~~~~~"
            Case STRECKE_WUESTE
                Console.ForegroundColor = ConsoleColor.Yellow
                nameAnzeige = "---  WUESTE  ---"
                rahmen = "----------------"
            Case STRECKE_AUTOBAHN
                Console.ForegroundColor = ConsoleColor.White
                nameAnzeige = "===  AUTOBAHN  ==="
                rahmen = "=================="
        End Select

        Zentriert_Schreiben(rahmen, 9)
        Zentriert_Schreiben(nameAnzeige, 10)
        Zentriert_Schreiben(rahmen, 11)

        Console.ForegroundColor = ConsoleColor.Gray
        Zentriert_Schreiben("Viel Erfolg – weiche den Hindernissen aus!", 13)

        Threading.Thread.Sleep(2000)
        Console.Clear()
    End Sub


    ' Fahrzeug (2x2 ASCII)
    ' Ki-Entwurf - von uns integriert sowie spezfisch angepasst für das Spiel.

    Function Fahrzeug_Zeile1(ByVal id As Integer) As String
        Select Case id
            Case 1 : Return "/##\"
            Case 2 : Return "[BB]"
            Case 3 : Return " /\ "
            Case 4 : Return "=$$="
            Case Else : Return "/##\"
        End Select
    End Function

    Function Fahrzeug_Zeile2(ByVal id As Integer) As String
        Select Case id
            Case 1 : Return "\##/"
            Case 2 : Return "|BB|"
            Case 3 : Return "(oo)"
            Case 4 : Return "=$$="
            Case Else : Return "\##/"
        End Select
    End Function

    Function Fahrzeug_Farbe(ByVal id As Integer) As ConsoleColor
        Select Case id
            Case 1 : Return ConsoleColor.White
            Case 2 : Return ConsoleColor.Green
            Case 3 : Return ConsoleColor.Cyan
            Case 4 : Return ConsoleColor.Yellow
            Case Else : Return ConsoleColor.White
        End Select
    End Function

    Function Fahrzeug_Breite(ByVal id As Integer) As Integer
        Return Fahrzeug_Zeile2(id).Length
    End Function

    Function Fahrzeug_Name(ByVal id As Integer) As String
        Select Case id
            Case 1 : Return "Standard  "
            Case 2 : Return "Buggy     "
            Case 3 : Return "Klein     "
            Case 4 : Return "Gold [$]  "
            Case Else : Return "?"
        End Select
    End Function

    Function Fahrzeug_Ultimate_Info(ByVal id As Integer) As String
        Select Case id
            Case 1 : Return "Vollbremsung: langsam + unverwundbar [LEERTASTE]"
            Case 2 : Return "5 Sek. unverwundbar [LEERTASTE]"
            Case 3 : Return "Doppelsprung 2 Felder [LEERTASTE]"
            Case 4 : Return "Goldraeumung: Hindernisse weg [LEERTASTE]"
            Case Else : Return ""
        End Select
    End Function

    Sub Fahrzeug_Zeichnen(ByVal spalte As Integer,
                          ByVal id As Integer,
                          ByVal ultimateAktiv As Boolean,
                          ByVal schildAktiv As Boolean,
                          ByVal strecke As Integer)
        Dim farbe As ConsoleColor = Fahrzeug_Farbe(id)
        If ultimateAktiv Then farbe = ConsoleColor.Yellow   ' Ultimate = gelb
        If schildAktiv Then farbe = ConsoleColor.Cyan

        Console.SetCursorPosition(spalte, ZEILE_MAX - 2)
        Console.ForegroundColor = farbe
        Console.Write(Fahrzeug_Zeile1(id))

        Console.SetCursorPosition(spalte, ZEILE_MAX - 1)
        Console.ForegroundColor = farbe
        Console.Write(Fahrzeug_Zeile2(id))

        Strecke_Farben_Setzen(strecke)
    End Sub

    Sub Fahrzeug_Loeschen(ByVal spalte As Integer, ByVal id As Integer)
        Dim breite As Integer = Fahrzeug_Breite(id)
        Dim leerzeichen As String = New String(" "c, breite)
        Console.SetCursorPosition(spalte, ZEILE_MAX - 2)
        Console.Write(leerzeichen)
        Console.SetCursorPosition(spalte, ZEILE_MAX - 1)
        Console.Write(leerzeichen)
    End Sub


    Function Startposition_Berechnen(ByVal spielerPlatz As Integer,
                                     ByVal leitLinks As Integer,
                                     ByVal leitRechts As Integer) As Integer
        Dim fahrbahnBreite As Integer = leitRechts - leitLinks - 1
        Dim drittel As Integer = fahrbahnBreite \ 3
        Select Case spielerPlatz
            Case 1 : Return leitLinks + 1 + (drittel \ 2)
            Case 2 : Return leitLinks + 1 + drittel + (drittel \ 2)
            Case 3 : Return leitLinks + 1 + (drittel * 2) + (drittel \ 2)
            Case Else : Return leitLinks + 1 + drittel + (drittel \ 2)
        End Select
    End Function

#End Region

#Region "Streckengenerator"

    ' Nächstes Streckensegment würfeln
    ' Ki-Entwurf - von uns integriert sowie spezfisch angepasst für das Spiel.

    Sub Segment_Neu_Wuerfeln(ByRef zielBreite As Double,
                              ByRef zielMitte As Double,
                              ByRef laenge As Integer)
        Randomize()
        Dim typ As Integer = CInt(Math.Floor(VBMath.Rnd() * 9))
        Select Case typ
            Case 0
                zielBreite = BREITE_BREIT : zielMitte = MITTE_MITTE
                laenge = SEG_LAENGE_MIN + CInt(VBMath.Rnd() * 10)
            Case 1
                zielBreite = BREITE_MITTEL : zielMitte = MITTE_LINKS
                laenge = SEG_LAENGE_MIN + CInt(VBMath.Rnd() * (SEG_LAENGE_MAX - SEG_LAENGE_MIN))
            Case 2
                zielBreite = BREITE_MITTEL : zielMitte = MITTE_RECHTS
                laenge = SEG_LAENGE_MIN + CInt(VBMath.Rnd() * (SEG_LAENGE_MAX - SEG_LAENGE_MIN))
            Case 3
                zielBreite = BREITE_MITTEL - 3 : zielMitte = MITTE_WEIT_LINKS
                laenge = SEG_LAENGE_MIN + CInt(VBMath.Rnd() * 10)
            Case 4
                zielBreite = BREITE_MITTEL - 3 : zielMitte = MITTE_WEIT_RECHTS
                laenge = SEG_LAENGE_MIN + CInt(VBMath.Rnd() * 10)
            Case 5
                zielBreite = BREITE_SCHMAL : zielMitte = MITTE_MITTE
                laenge = SEG_LAENGE_MIN
            Case 6
                zielBreite = BREITE_SCHMAL + 3 : zielMitte = MITTE_WEIT_LINKS
                laenge = SEG_LAENGE_MIN + CInt(VBMath.Rnd() * 8)
            Case 7
                zielBreite = BREITE_SCHMAL + 3 : zielMitte = MITTE_WEIT_RECHTS
                laenge = SEG_LAENGE_MIN + CInt(VBMath.Rnd() * 8)
            Case 8
                Dim schikane As Integer = CInt(VBMath.Rnd() * 1)
                zielBreite = BREITE_MITTEL - 2
                zielMitte = If(schikane = 0, MITTE_WEIT_LINKS, MITTE_WEIT_RECHTS)
                laenge = SEG_LAENGE_MIN
        End Select
    End Sub


    ' Erzeugt eine Zeile der Strecke
    ' nurStraße=True -> keine Hindernisse/Items (Spielstart)
    ' Ki-Entwurf - von uns integriert sowie spezfisch angepasst für das Spiel.

    Sub Erzeuge_Zeile(ByRef Zeile() As Char,
                      ByVal aktBreite As Double,
                      ByVal aktMitte As Double,
                      ByVal strecke As Integer,
                      ByVal hindernisChance As Integer,
                      ByVal startZeileNr As Integer,
                      ByVal nurStrasse As Boolean)

        Dim i As Integer
        Dim halbBreite As Integer = CInt(aktBreite) \ 2
        Dim mitte As Integer = CInt(STRASSE_MITTE_STANDARD + aktMitte)
        Dim leitLinks As Integer = mitte - halbBreite
        Dim leitRechts As Integer = mitte + halbBreite

        If leitLinks < 1 Then leitLinks = 1
        If leitRechts > SPALTE_MAX - 1 Then leitRechts = SPALTE_MAX - 1
        If leitRechts <= leitLinks + 1 Then leitRechts = leitLinks + 2

        For i = 0 To SPALTE_MAX
            Zeile(i) = " "c
        Next

        Zeile(leitLinks) = LEITPLANKE_ZEICHEN
        Zeile(leitRechts) = LEITPLANKE_ZEICHEN

        If startZeileNr > 0 Then
            Dim fahrbahnBreite As Integer = leitRechts - leitLinks - 1
            Dim drittel As Integer = fahrbahnBreite \ 3
            Dim posMitte As Integer = leitLinks + 1 + drittel + (drittel \ 2)
            Dim posLinks As Integer = leitLinks + 1 + (drittel \ 2)
            Dim posRechts As Integer = leitLinks + 1 + (drittel * 2) + (drittel \ 2)

            Select Case startZeileNr
                Case 1
                    If posMitte - 1 > leitLinks And posMitte < leitRechts Then
                        Zeile(posMitte - 1) = "P"c
                        Zeile(posMitte) = "1"c
                    End If
                Case 2
                    If posLinks - 1 > leitLinks And posLinks < leitRechts Then
                        Zeile(posLinks - 1) = "P"c
                        Zeile(posLinks) = "2"c
                    End If
                Case 3
                    If posRechts - 1 > leitLinks And posRechts < leitRechts Then
                        Zeile(posRechts - 1) = "P"c
                        Zeile(posRechts) = "3"c
                    End If
            End Select
        ElseIf nurStrasse Then
            ' Leitplanken reichen – kein Spawn
            ' Fehlerbehebung durch Ki-Einsatz (Zufällige Generierung der Leitplankenpositionen).

        Else
            Randomize()
            Dim fahrbahnBreite As Integer = leitRechts - leitLinks - 1
            If fahrbahnBreite > 3 Then
                If CInt(VBMath.Rnd() * ITEM_CHANCE) = 0 Then
                    Dim itemPos As Integer = leitLinks + 1 +
                        CInt(Math.Floor(VBMath.Rnd() * (fahrbahnBreite - 2)))
                    If itemPos > leitLinks And itemPos + 2 < leitRechts Then
                        If Zeile(itemPos) = " "c And
                           Zeile(itemPos + 1) = " "c And
                           Zeile(itemPos + 2) = " "c Then
                            Zeile(itemPos) = ITEM_LINKS
                            Zeile(itemPos + 1) = ITEM_MITTE
                            Zeile(itemPos + 2) = ITEM_RECHTS
                        End If
                    End If
                End If
            End If

            Dim fahrbahnFrei As Integer = leitRechts - leitLinks - 1
            If fahrbahnFrei > 2 Then
                If CInt(VBMath.Rnd() * hindernisChance) = 0 Then
                    Dim hindPos As Integer = leitLinks + 1 +
                        CInt(Math.Floor(VBMath.Rnd() * (fahrbahnFrei - 1)))
                    If hindPos >= leitLinks + 1 And hindPos + 1 <= leitRechts - 1 Then
                        If Zeile(hindPos) = " "c And Zeile(hindPos + 1) = " "c Then
                            Zeile(hindPos) = HINDERNIS_ZEICHEN
                            Zeile(hindPos + 1) = HINDERNIS_ZEICHEN
                        End If
                    End If
                End If
            End If


            ' Boost-Feld (2 Zeichen breit) zufällig auf freie Straße setzen
            ' Ki-Entwurf - von uns integriert sowie spezifisch angepasst für das Spiel.

            Dim fahrbahnBoost As Integer = leitRechts - leitLinks - 1
            If fahrbahnBoost > 2 Then
                If CInt(VBMath.Rnd() * BOOST_CHANCE) = 0 Then
                    Dim boostPos As Integer = leitLinks + 1 +
                        CInt(Math.Floor(VBMath.Rnd() * (fahrbahnBoost - 1)))
                    If boostPos >= leitLinks + 1 And boostPos + 1 <= leitRechts - 1 Then
                        If Zeile(boostPos) = " "c And Zeile(boostPos + 1) = " "c Then
                            Zeile(boostPos) = BOOST_ZEICHEN
                            Zeile(boostPos + 1) = BOOST_ZEICHEN
                        End If
                    End If
                End If
            End If
        End If
    End Sub

#End Region

#Region "SpielMechanik"

    ' Spielfeld zeichnen (auch nach Pause)
    ' Ki-Entwurf - von uns integriert sowie spezifisch angepasst für das Spiel.

    Sub Spielfeld_Rendern(ByVal spielfeld(,) As Char,
                          ByVal curbPhase As Integer,
                          ByVal strecke As Integer)
        Dim z, s As Integer
        Console.SetCursorPosition(0, 0)
        For z = 0 To ZEILE_MAX - 3
            For s = 0 To SPALTE_MAX
                Dim zelle As Char = spielfeld(z, s)
                Select Case zelle
                    Case LEITPLANKE_ZEICHEN
                        Curb_Farbe_Setzen((curbPhase + z) Mod 2)
                        Console.Write(zelle)
                        Strecke_Farben_Setzen(strecke)
                    Case ITEM_LINKS, ITEM_MITTE, ITEM_RECHTS
                        Console.BackgroundColor = ConsoleColor.DarkYellow
                        Console.ForegroundColor = ConsoleColor.White
                        Console.Write(zelle)
                        Strecke_Farben_Setzen(strecke)
                    Case HINDERNIS_ZEICHEN
                        Console.ForegroundColor = ConsoleColor.Red
                        Console.Write(zelle)
                        Strecke_Farben_Setzen(strecke)
                    Case BOOST_ZEICHEN
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.ForegroundColor = ConsoleColor.Green
                        Console.Write(zelle)
                        Strecke_Farben_Setzen(strecke)
                    Case STARTLINIE_ZEICHEN


                        ' Schwarz-weiss kariert (je nach Spalte abwechselnd)
                        ' Fehlerbehebung durch Ki-Einsatz (richtige Reihenfolge der Farbanordnung)

                        If s Mod 2 = 0 Then
                            Console.BackgroundColor = ConsoleColor.White
                            Console.ForegroundColor = ConsoleColor.White
                        Else
                            Console.BackgroundColor = ConsoleColor.Black
                            Console.ForegroundColor = ConsoleColor.Black
                        End If
                        Console.Write(zelle)
                        Strecke_Farben_Setzen(strecke)
                    Case MEILENSTEIN_ZEICHEN
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.Write(zelle)
                        Strecke_Farben_Setzen(strecke)
                    Case "P"c
                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.Write(zelle)
                        Strecke_Farben_Setzen(strecke)
                    Case "1"c
                        Console.ForegroundColor = ConsoleColor.Red
                        Console.Write(zelle)
                        Strecke_Farben_Setzen(strecke)
                    Case "2"c
                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.Write(zelle)
                        Strecke_Farben_Setzen(strecke)
                    Case "3"c
                        Console.ForegroundColor = ConsoleColor.Green
                        Console.Write(zelle)
                        Strecke_Farben_Setzen(strecke)
                    Case Else
                        Console.Write(zelle)
                End Select
            Next
            Console.WriteLine()
        Next
    End Sub


    ' Pause (Taste P im Spiel)
    ' Ki-Entwurf - von uns integriert sowie spezifisch angepasst für das Spiel.

    Sub Pause_Anzeigen(ByVal spielfeld(,) As Char,
                       ByVal curbPhase As Integer,
                       ByVal strecke As Integer)
        Musik_Stoppen()
        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()

        Console.ForegroundColor = ConsoleColor.Yellow
        Zentriert_Schreiben("+-------------------------------+", 9)
        Zentriert_Schreiben("|          P A U S E            |", 10)
        ' Pause-Fenster
        Zentriert_Schreiben("|                               |", 11)
        Zentriert_Schreiben("| Beliebige Taste = Fortsetzen  |", 12)
        Zentriert_Schreiben("+-------------------------------+", 13)
        Console.ForegroundColor = ConsoleColor.Gray
        Zentriert_Schreiben("Steuerung: Pfeiltasten | Ultimate: Leertaste", 15)


        ' Auf eine beliebige Taste warten

        Console.ReadKey(True)


        ' Spielmusik wieder starten und Spielfeld neu aufbauen
        ' Ki-Entwurf - von uns integriert sowie spezifisch angepasst für das Spiel.

        Musik_Starten("Spielsound.wav")
        Strecke_Farben_Setzen(strecke)
        Console.Clear()
        Spielfeld_Rendern(spielfeld, curbPhase, strecke)
    End Sub


    ' Ultimate je Fahrzeug (Rückgabe = Unverwundbar-Ticks)
    ' Ki-Entwurf - von uns integriert sowie spezifisch angepasst für das Spiel.
    Function Ultimate_Ausfuehren(ByVal fahrzeug As Integer,
                                  ByRef spielfigur_spalte As Integer,
                                  ByRef slowmoTicks As Integer,
                                  ByRef keinHindernisMeter As Integer,
                                  ByRef spielfeld(,) As Char) As Integer
        Select Case fahrzeug
            Case 1


                ' Standard – Vollbremsung: Zeitlupe + Unverwundbarkeit

                slowmoTicks = ULT_VOLLBREMSUNG_TICKS
                Return ULT_VOLLBREMSUNG_TICKS
            Case 2


                ' Buggy – 5 Sek. unverwundbar

                Return ULT_BUGGY_TICKS
            Case 3


                ' Klein – Doppelsprung 2 Felder nach links

                spielfigur_spalte = Math.Max(0, spielfigur_spalte - 2)
                Return 0
            Case 4


                ' Gold – Goldräumung: alle Hindernisse auf dem Bildschirm
                ' löschen und für eine kurze Schonzeit keine neuen spawnen

                Dim z, s As Integer
                For z = 0 To ZEILE_MAX
                    For s = 0 To SPALTE_MAX
                        If spielfeld(z, s) = HINDERNIS_ZEICHEN Then
                            spielfeld(z, s) = " "c
                        End If
                    Next
                Next
                keinHindernisMeter = GOLD_RAEUMUNG_METER
                Return 0
            Case Else
                Return 0
        End Select
    End Function


    ' Schuss nach oben (Pfeil hoch, braucht Munition)
    ' Ki-Entwurf - von uns integriert sowie spezifisch angepasst für das Spiel.

    Sub Schuss_Abfeuern(ByVal spielfigur_spalte As Integer,
                        ByVal fahrzeugBreite As Integer,
                        ByVal curbPhase As Integer,
                        ByVal strecke As Integer,
                        ByRef spielfeld(,) As Char)

        Dim z, s As Integer
        Dim trefferZeile As Integer = -1
        Dim trefferSpalte As Integer = -1


        ' Nächstes Hindernis oberhalb des Fahrzeugs suchen
        ' (von knapp über dem Auto nach oben)

        For z = ZEILE_MAX - 3 To 0 Step -1
            For s = spielfigur_spalte To spielfigur_spalte + fahrzeugBreite - 1
                If s >= 0 And s <= SPALTE_MAX Then
                    If spielfeld(z, s) = HINDERNIS_ZEICHEN Then
                        trefferZeile = z
                        trefferSpalte = s
                        Exit For
                    End If
                End If
            Next
            If trefferZeile >= 0 Then Exit For
        Next


        ' Schuss-Spalte = Mitte des Fahrzeugs

        Dim schussSpalte As Integer = spielfigur_spalte + (fahrzeugBreite \ 2)


        ' Strahl zeichnen (vom Fahrzeug bis zum Treffer bzw. nach ganz oben)

        Dim bisZeile As Integer = If(trefferZeile >= 0, trefferZeile, 0)
        Console.ForegroundColor = ConsoleColor.Red
        For z = ZEILE_MAX - 3 To bisZeile Step -1
            If schussSpalte >= 0 And schussSpalte <= SPALTE_MAX Then
                Console.SetCursorPosition(schussSpalte, z)
                Console.Write("|"c)
            End If
        Next
        Strecke_Farben_Setzen(strecke)
        Threading.Thread.Sleep(80)


        ' Getroffenes Hindernis zerstören (Hindernisse sind 2 Zeichen breit)

        If trefferZeile >= 0 Then
            Dim hindLinks As Integer = trefferSpalte
            If hindLinks > 0 AndAlso spielfeld(trefferZeile, hindLinks - 1) = HINDERNIS_ZEICHEN Then
                hindLinks -= 1
            End If
            spielfeld(trefferZeile, hindLinks) = " "c
            If hindLinks + 1 <= SPALTE_MAX Then
                spielfeld(trefferZeile, hindLinks + 1) = " "c
            End If
            g_statHindernisZerstoert += 1
        End If


        ' Spielfeld neu aufbauen, damit der Strahl wieder verschwindet

        Strecke_Farben_Setzen(strecke)
        Spielfeld_Rendern(spielfeld, curbPhase, strecke)
    End Sub

#End Region

#Region "MetaSpiel"

    ' Statistik (Menü-Option)
    ' Fehlerbehebung durch Ki-Einsatz (Strukturierung der Menü-Option)
    Sub Statistik_Zuruecksetzen()
        g_statSpiele = 0
        g_statGesamtMeter = 0
        g_statBesteMeter = 0
        g_statItems = 0
        g_statHindernisse = 0
        g_statHindernisZerstoert = 0
        g_streckeGefahren(STRECKE_EIS) = False
        g_streckeGefahren(STRECKE_WUESTE) = False
        g_streckeGefahren(STRECKE_AUTOBAHN) = False
    End Sub

    Sub Statistik_Anzeigen()
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Clear()

        Zentriert_Schreiben("===  S T A T I S T I K  ===", 2)

        Dim schnitt As Integer = 0
        If g_statSpiele > 0 Then
            schnitt = g_statGesamtMeter \ g_statSpiele   ' kein Division-by-Zero
        End If

        Console.ForegroundColor = ConsoleColor.White
        Console.SetCursorPosition(20, 5)
        Console.WriteLine("Gespielte Runden   : " & g_statSpiele)
        Console.SetCursorPosition(20, 6)
        Console.WriteLine("Gesamtstrecke      : " & g_statGesamtMeter & " m")
        Console.SetCursorPosition(20, 7)
        Console.WriteLine("Beste Strecke      : " & g_statBesteMeter & " m")
        Console.SetCursorPosition(20, 8) : Console.WriteLine("Schnitt pro Runde  : " & schnitt & " m")
        Console.SetCursorPosition(20, 9) : Console.WriteLine("Items gesammelt    : " & g_statItems)
        Console.SetCursorPosition(20, 10) : Console.WriteLine("Hindernis-Treffer  : " & g_statHindernisse)
        Console.SetCursorPosition(20, 11) : Console.WriteLine("Hindernis zerstoert: " & g_statHindernisZerstoert)

        Console.ForegroundColor = ConsoleColor.Yellow
        Console.SetCursorPosition(20, 13) : Console.WriteLine("Befahrene Strecken :")
        Console.ForegroundColor = ConsoleColor.White
        Console.SetCursorPosition(22, 14) : Console.WriteLine("Eisstrecke : " & If(g_streckeGefahren(STRECKE_EIS), "Ja", "Nein"))
        Console.SetCursorPosition(22, 15) : Console.WriteLine("Wueste     : " & If(g_streckeGefahren(STRECKE_WUESTE), "Ja", "Nein"))
        Console.SetCursorPosition(22, 16) : Console.WriteLine("Autobahn   : " & If(g_streckeGefahren(STRECKE_AUTOBAHN), "Ja", "Nein"))

        Console.ForegroundColor = ConsoleColor.Gray
        Zentriert_Schreiben("[ENTER] Zurueck", 19)
        Console.CursorVisible = True
        Console.ReadLine()
        Console.CursorVisible = False
    End Sub

    ' Achievements – Name, Beschreibung, Prüfung nach jeder Runde

    Function Achievement_Name(ByVal id As Integer) As String
        Select Case id
            Case 0 : Return "Erste Fahrt"
            Case 1 : Return "Halbe Strecke"
            Case 2 : Return "Goldjaeger"
            Case 3 : Return "Sammler"
            Case 4 : Return "Veteran"
            Case 5 : Return "Weltreisender"
            Case 6 : Return "Dauerbrenner"
            Case Else : Return "?"
        End Select
    End Function

    Function Achievement_Beschreibung(ByVal id As Integer) As String
        Select Case id
            Case 0 : Return "Spiele deine erste Runde"
            Case 1 : Return "Erreiche 250 Meter in einer Runde"
            Case 2 : Return "Schalte das goldene Fahrzeug frei (400 m)"
            Case 3 : Return "Sammle insgesamt 10 Items ein"
            Case 4 : Return "Spiele insgesamt 10 Runden"
            Case 5 : Return "Fahre alle drei Strecken mindestens einmal"
            Case 6 : Return "Lege insgesamt 1000 Meter zurueck"
            Case Else : Return ""
        End Select
    End Function


    ' Schaltet einen Erfolg frei, wenn die Bedingung erstmals erfüllt ist.
    ' Ki-Entwurf - von uns integriert sowie spezifisch angepasst für das Spiel.

    Sub Pruefe_Achievement(ByVal id As Integer, ByVal bedingung As Boolean)
        ' Nur einmal freischalten
        If bedingung And Not g_achFreigeschaltet(id) Then
            g_achFreigeschaltet(id) = True
            If g_neuAchievements <> "" Then g_neuAchievements &= ", "
            g_neuAchievements &= Achievement_Name(id)
        End If
    End Sub


    ' Prüft alle Erfolge. g_neuAchievements enthält danach die Namen der in dieser Runde neu freigeschalteten Erfolge.
    ' Ki-Entwurf - von uns integriert sowie spezifisch angepasst für das Spiel.
    Sub Achievements_Pruefen()
        g_neuAchievements = ""
        Pruefe_Achievement(0, g_statSpiele >= 1)
        Pruefe_Achievement(1, g_statBesteMeter >= 250)
        Pruefe_Achievement(2, g_goldFreigeschaltet)
        Pruefe_Achievement(3, g_statItems >= 10)
        Pruefe_Achievement(4, g_statSpiele >= 10)
        Pruefe_Achievement(5, g_streckeGefahren(STRECKE_EIS) And
                               g_streckeGefahren(STRECKE_WUESTE) And
                               g_streckeGefahren(STRECKE_AUTOBAHN))
        Pruefe_Achievement(6, g_statGesamtMeter >= 1000)
    End Sub

    Sub Achievements_Anzeigen()
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Magenta
        Console.Clear()

        Zentriert_Schreiben("===  A C H I E V E M E N T S  ===", 1)


        ' Anzahl freigeschalteter Erfolge zählen

        Dim anzahl As Integer = 0
        For k As Integer = 0 To ACH_ANZAHL - 1
            If g_achFreigeschaltet(k) Then anzahl += 1
        Next

        Console.ForegroundColor = ConsoleColor.Gray
        Zentriert_Schreiben("Freigeschaltet: " & anzahl & " / " & ACH_ANZAHL, 2)

        For k As Integer = 0 To ACH_ANZAHL - 1
            Dim zeile As Integer = 4 + (k * 2)
            If g_achFreigeschaltet(k) Then
                Console.ForegroundColor = ConsoleColor.Green
                Console.SetCursorPosition(8, zeile)
                Console.WriteLine("[X] " & Achievement_Name(k))
            Else
                Console.ForegroundColor = ConsoleColor.DarkGray
                Console.SetCursorPosition(8, zeile)
                Console.WriteLine("[ ] " & Achievement_Name(k))
            End If
            Console.ForegroundColor = ConsoleColor.DarkGray
            Console.SetCursorPosition(12, zeile + 1)
            Console.WriteLine(Achievement_Beschreibung(k))
        Next

        Console.ForegroundColor = ConsoleColor.Gray
        Zentriert_Schreiben("[ENTER] Zurueck", 4 + (ACH_ANZAHL * 2) + 1)
        Console.CursorVisible = True
        Console.ReadLine()
        Console.CursorVisible = False
    End Sub

    Sub Highscore_Speichern(ByVal name As String, ByVal meter As Integer)
        If meter >= GOLD_UNLOCK_METER Then g_goldFreigeschaltet = True
        If g_highscoreAnzahl < 5 Then
            g_highscores(g_highscoreAnzahl).name = name
            g_highscores(g_highscoreAnzahl).meter = meter
            g_highscoreAnzahl += 1
        Else
            Dim minIdx As Integer = 0
            For k As Integer = 1 To 4
                If g_highscores(k).meter < g_highscores(minIdx).meter Then minIdx = k
            Next
            If meter > g_highscores(minIdx).meter Then
                g_highscores(minIdx).name = name
                g_highscores(minIdx).meter = meter
            End If
        End If

        ' Tabelle nach Meter sortieren
        For k As Integer = 0 To g_highscoreAnzahl - 2
            For l As Integer = 0 To g_highscoreAnzahl - 2 - k
                If g_highscores(l).meter < g_highscores(l + 1).meter Then
                    Dim tmp As HighscoreEintrag = g_highscores(l)
                    g_highscores(l) = g_highscores(l + 1)
                    g_highscores(l + 1) = tmp
                End If
            Next
        Next
    End Sub

    Sub Highscore_Anzeigen()
        Musik_Starten("Highscore.wav")   ' eigene Musik
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Yellow
        Console.Clear()
        Zentriert_Schreiben("=== HIGHSCORE ===", 3)
        Console.SetCursorPosition(20, 5) : Console.WriteLine("  #   Name              Meter")
        Console.SetCursorPosition(20, 6) : Console.WriteLine("  -   ----------------  -----")
        If g_highscoreAnzahl = 0 Then
            Console.SetCursorPosition(20, 7) : Console.WriteLine("  Noch keine Eintraege.")
        Else
            For k As Integer = 0 To g_highscoreAnzahl - 1
                Console.SetCursorPosition(20, 7 + k)
                Console.WriteLine("  " & (k + 1) & ".  " &
                    g_highscores(k).name.PadRight(16) & "  " &
                    g_highscores(k).meter & " m")
            Next
        End If
        Console.ForegroundColor = ConsoleColor.Gray
        Zentriert_Schreiben("[ENTER] Zurueck", 15)
        Console.CursorVisible = True
        Console.ReadLine()
        Console.CursorVisible = False
        Musik_Stoppen()
    End Sub

    ' Game Over Screen
    ' Fehlerbehebung durch Ki-Einsatz (Anzeige des Game Over Screens nach jeder Runde, damit Spieler sofortiges Feedback erhält).

    Sub Game_Over(ByVal meter As Integer)
        Musik_Stoppen()
        Sound_GameOver()

        g_statSpiele += 1   ' Statistik für diese Runde
        g_statGesamtMeter += meter
        If meter > g_statBesteMeter Then g_statBesteMeter = meter
        g_streckeGefahren(g_strecke) = True

        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()

        Console.ForegroundColor = ConsoleColor.Red
        Zentriert_Schreiben("*" & New String("=", 51) & "*", 2)
        Zentriert_Schreiben(" ██████╗  █████╗ ███╗   ███╗███████╗", 4)
        Zentriert_Schreiben("██╔════╝ ██╔══██╗████╗ ████║██╔════╝", 5)
        Zentriert_Schreiben("██║  ███╗███████║██╔████╔██║█████╗  ", 6)
        Zentriert_Schreiben("██║   ██║██╔══██║██║╚██╔╝██║██╔══╝  ", 7)
        Zentriert_Schreiben("╚██████╔╝██║  ██║██║ ╚═╝ ██║███████╗", 8)
        Zentriert_Schreiben(" ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝", 9)

        Console.ForegroundColor = ConsoleColor.DarkRed
        Zentriert_Schreiben("██████╗ ██╗   ██╗███████╗██████╗ ", 11)
        Zentriert_Schreiben("██╔══██╗██║   ██║██╔════╝██╔══██╗", 12)
        Zentriert_Schreiben("██║  ██║██║   ██║█████╗  ██████╔╝", 13)
        Zentriert_Schreiben("██║  ██║╚██╗ ██╔╝██╔══╝  ██╔══██╗", 14)
        Zentriert_Schreiben("██████╔╝ ╚████╔╝ ███████╗██║  ██║", 15)
        Zentriert_Schreiben("╚═════╝   ╚═══╝  ╚══════╝╚═╝  ╚═╝", 16)

        Console.ForegroundColor = ConsoleColor.Red
        Zentriert_Schreiben("*" & New String("=", 51) & "*", 17)

        Console.ForegroundColor = ConsoleColor.Yellow
        Zentriert_Schreiben("Zurueckgelegte Strecke:  " & meter & " Meter", 19)

        If meter >= GOLD_UNLOCK_METER And Not g_goldFreigeschaltet Then
            Console.ForegroundColor = ConsoleColor.Yellow
            Zentriert_Schreiben("★  GOLDENES FAHRZEUG FREIGESCHALTET!  ★", 20)
        End If


        ' Neue Achievements prüfen und ggf. anzeigen

        Achievements_Pruefen()
        If g_neuAchievements <> "" Then
            Console.ForegroundColor = ConsoleColor.Green
            Zentriert_Schreiben("Neuer Erfolg: " & g_neuAchievements, 18)
        End If

        Console.ForegroundColor = ConsoleColor.White
        Zentriert_Schreiben("Deinen Namen eingeben:", 21)

        Dim nameText As String = "Deinen Namen eingeben:"
        Dim nameSpalte As Integer = (SPALTE_MAX \ 2) - (nameText.Length \ 2)
        Console.SetCursorPosition(nameSpalte, 22)
        Console.CursorVisible = True
        Dim name As String = Console.ReadLine()
        Console.CursorVisible = False
        If name Is Nothing OrElse name.Trim() = "" Then name = "Unbekannt"
        Highscore_Speichern(name, meter)

        Console.ForegroundColor = ConsoleColor.Gray
        Zentriert_Schreiben("[ENTER] Zurueck zum Menue", 23)
        Console.CursorVisible = True
        Console.ReadLine()
        Console.CursorVisible = False
    End Sub

#End Region

#Region "Spielablauf"

    ' Hauptschleife während einer Runde
    ' Ki-Entwurf - von uns integriert sowie spezifisch angepasst für das Spiel.

    Sub Spielablauf()
        Dim leben As Integer = g_startLeben
        Dim spielfeld(ZEILE_MAX, SPALTE_MAX) As Char
        Dim zeile(SPALTE_MAX) As Char
        Dim z, s, i As Integer
        Dim taste As Integer
        Dim meter As Integer = 0

        Dim aktBreite As Double = STRASSE_BREITE_START
        Dim aktMitte As Double = MITTE_MITTE
        Dim zielBreite As Double = STRASSE_BREITE_START
        Dim zielMitte As Double = MITTE_MITTE
        Dim segRestlaenge As Integer = SEG_LAENGE_MAX
        Dim segLaenge As Integer = SEG_LAENGE_MAX

        Dim schildAktiv As Boolean = False
        Dim schildEnde As DateTime = DateTime.MinValue
        Dim ultimateAktiv As Boolean = False
        Dim ultimateTicks As Integer = 0
        Dim ultimateVerfuegbar As Boolean = False
        Dim hindernisSchutz As Integer = 0
        Dim slowmoTicks As Integer = 0
        Dim keinHindernisMeter As Integer = 0
        Dim schussMunition As Integer = 0
        Dim boostEnde As DateTime = DateTime.MinValue
        Dim meilensteinText As String = ""
        Dim meilensteinTicks As Integer = 0

        Dim hindernisChance As Integer = HINDERNIS_CHANCE_START
        Dim naechsteStufe As Integer = SCHWIERIGKEIT_INTERVALL
        Dim schwierigkeitStufe As Integer = 1
        Dim curbPhase As Integer = 0

        g_itemMeldung = ""
        g_itemMeldungTicks = 0

        Strecke_Einleitung(g_strecke)
        Dim spielerPlatz As Integer = Startaufstellung_Anzeigen()

        Musik_Starten("Spielsound.wav")

        Strecke_Farben_Setzen(g_strecke)
        Console.Clear()

        Dim halbBreiteStart As Integer = CInt(aktBreite) \ 2
        Dim mitteStart As Integer = CInt(STRASSE_MITTE_STANDARD + aktMitte)
        Dim leitLinksStart As Integer = Math.Max(1, mitteStart - halbBreiteStart)
        Dim leitRechtsStart As Integer = Math.Min(SPALTE_MAX - 1, mitteStart + halbBreiteStart)
        Dim spielfigur_spalte As Integer = Startposition_Berechnen(
            spielerPlatz, leitLinksStart, leitRechtsStart)

        Dim fahrzeugBreite As Integer = Fahrzeug_Breite(g_fahrzeug)


        ' Spielfeld: erst nur Straße, P1/P2/P3-Markierungen unten


        For z = 0 To ZEILE_MAX
            Dim startNr As Integer = 0
            If z = ZEILE_MAX - 6 Then startNr = 1
            If z = ZEILE_MAX - 4 Then startNr = 2
            If z = ZEILE_MAX - 2 Then startNr = 3
            Erzeuge_Zeile(zeile, aktBreite, aktMitte, g_strecke,
                          hindernisChance, startNr, True)
            For s = 0 To SPALTE_MAX
                spielfeld(z, s) = zeile(s)
            Next
        Next


        ' Startlinie quer über die Straße setzen (knapp über den
        ' Startplätzen). Sie scrollt beim Losfahren nach unten weg.

        Dim startlinieZeile As Integer = ZEILE_MAX - 8
        For s = leitLinksStart + 1 To leitRechtsStart - 1
            spielfeld(startlinieZeile, s) = STARTLINIE_ZEICHEN
        Next

        Dim wartezeit As Single
        If g_ccm = 50 Then
            wartezeit = CCM_50_WARTEZEIT
        ElseIf g_ccm = 150 Then
            wartezeit = CCM_150_WARTEZEIT
        Else
            wartezeit = CCM_100_WARTEZEIT
        End If

        Do
            segRestlaenge -= 1
            If segRestlaenge <= 0 Then
                Segment_Neu_Wuerfeln(zielBreite, zielMitte, segLaenge)
                segRestlaenge = segLaenge
            End If

            aktBreite += (zielBreite - aktBreite) * GLAETTUNG
            aktMitte += (zielMitte - aktMitte) * GLAETTUNG
            If aktBreite < STRASSE_BREITE_MIN Then aktBreite = STRASSE_BREITE_MIN
            If aktBreite > STRASSE_BREITE_MAX Then aktBreite = STRASSE_BREITE_MAX

            If meter >= naechsteStufe AndAlso hindernisChance > HINDERNIS_CHANCE_MIN Then
                hindernisChance -= 1
                schwierigkeitStufe += 1
                naechsteStufe += SCHWIERIGKEIT_INTERVALL
            End If

            curbPhase += 1


            ' Während der Goldrämungs-Schonzeit keine neuen Hindernisse:
            ' dazu eine sehr hohe Chance einsetzen (praktisch nie ein Treffer).
            ' Items spawnen weiterhin normal.

            Dim effektiveChance As Integer = hindernisChance
            If keinHindernisMeter > 0 Then
                effektiveChance = 1000000
                keinHindernisMeter -= 1
            End If
            Erzeuge_Zeile(zeile, aktBreite, aktMitte, g_strecke, effektiveChance, 0, False)

            For z = ZEILE_MAX To 1 Step -1
                For s = 0 To SPALTE_MAX
                    spielfeld(z, s) = spielfeld(z - 1, s)
                Next
            Next
            For s = 0 To SPALTE_MAX
                spielfeld(0, s) = zeile(s)
            Next

            meter += 1
            If hindernisSchutz > 0 Then hindernisSchutz -= 1
            If g_itemMeldungTicks > 0 Then g_itemMeldungTicks -= 1
            If g_itemMeldungTicks = 0 Then g_itemMeldung = ""


            ' Alle 100 m: Meilenstein-Linie quer über die Strasse in die
            ' oberste Zeile setzen und kurz einen Banner anzeigen.

            If (meter + KOLLISIONS_ZEILE) Mod MEILENSTEIN_INTERVALL = 0 Then
                Dim mlLinks As Integer = -1
                Dim mlRechts As Integer = -1
                For s = 0 To SPALTE_MAX
                    If spielfeld(0, s) = LEITPLANKE_ZEICHEN Then
                        If mlLinks < 0 Then mlLinks = s
                        mlRechts = s
                    End If
                Next
                If mlLinks >= 0 And mlRechts > mlLinks + 1 Then
                    For s = mlLinks + 1 To mlRechts - 1
                        spielfeld(0, s) = MEILENSTEIN_ZEICHEN
                    Next
                End If
                meilensteinText = (meter + KOLLISIONS_ZEILE) & " m"
                meilensteinTicks = 12
            End If


            ' Spielfeld zeichnen (eigene Sub)

            Spielfeld_Rendern(spielfeld, curbPhase, g_strecke)


            ' Meilenstein-Banner über dem Spielfeld anzeigen

            If meilensteinTicks > 0 Then
                Console.BackgroundColor = ConsoleColor.Black
                Console.ForegroundColor = ConsoleColor.Yellow
                Zentriert_Schreiben(">> " & meilensteinText & " <<", 2)
                Strecke_Farben_Setzen(g_strecke)
                meilensteinTicks -= 1
            End If

            Dim leitplankeGetroffen As Boolean = False

            For i = 1 To BEWEGUNG_SPIELFIGUR
                taste = NO_KEY
                Fahrzeug_Loeschen(spielfigur_spalte, g_fahrzeug) ' Alte Position löschen (Doppelsprung)
                If Console.KeyAvailable Then
                    Dim cki As ConsoleKeyInfo = Console.ReadKey(True)
                    If cki.Key = ConsoleKey.LeftArrow Then
                        taste = CURSOR_LEFT
                    ElseIf cki.Key = ConsoleKey.RightArrow Then
                        taste = CURSOR_RIGHT
                    ElseIf cki.Key = ConsoleKey.Spacebar Then
                        If ultimateVerfuegbar And Not ultimateAktiv Then
                            Dim ticks As Integer = Ultimate_Ausfuehren(g_fahrzeug,
                                spielfigur_spalte, slowmoTicks,
                                keinHindernisMeter, spielfeld)
                            If ticks > 0 Then
                                ultimateAktiv = True
                                ultimateTicks = ticks
                            End If
                            ultimateVerfuegbar = False
                        End If
                    ElseIf cki.Key = ConsoleKey.P Then


                        ' Spiel pausieren

                        Pause_Anzeigen(spielfeld, curbPhase, g_strecke)
                    ElseIf cki.Key = ConsoleKey.UpArrow Then


                        ' Schießen, falls Munition vorhanden
                        ' Fehlerbehebung durch Ki-Einsatz (Munitionsverbrauch wird korrekt angezeigt)
                        If schussMunition > 0 Then
                            Schuss_Abfeuern(spielfigur_spalte, fahrzeugBreite,
                                            curbPhase, g_strecke, spielfeld)
                            schussMunition -= 1
                        End If
                    End If
                End If

                Dim aktLeitLinks As Integer = 0
                Dim aktLeitRechts As Integer = SPALTE_MAX

                For s = 0 To SPALTE_MAX
                    If spielfeld(KOLLISIONS_ZEILE, s) = LEITPLANKE_ZEICHEN Then
                        aktLeitLinks = s : Exit For
                    End If
                Next
                For s = SPALTE_MAX To 0 Step -1
                    If spielfeld(KOLLISIONS_ZEILE, s) = LEITPLANKE_ZEICHEN Then
                        aktLeitRechts = s : Exit For
                    End If
                Next



                If taste = CURSOR_LEFT Then spielfigur_spalte -= 1
                If taste = CURSOR_RIGHT Then spielfigur_spalte += 1

                If spielfigur_spalte <= aktLeitLinks Then
                    If Not ultimateAktiv And Not schildAktiv And Not leitplankeGetroffen Then
                        leben -= 1 : leitplankeGetroffen = True
                    End If
                    spielfigur_spalte = aktLeitLinks + 1
                End If

                If spielfigur_spalte + fahrzeugBreite - 1 >= aktLeitRechts Then
                    If Not ultimateAktiv And Not schildAktiv And Not leitplankeGetroffen Then
                        leben -= 1 : leitplankeGetroffen = True
                    End If
                    spielfigur_spalte = aktLeitRechts - fahrzeugBreite
                End If

                If spielfigur_spalte > aktLeitLinks And
                   spielfigur_spalte + fahrzeugBreite - 1 < aktLeitRechts Then
                    leitplankeGetroffen = False
                End If

                Fahrzeug_Zeichnen(spielfigur_spalte, g_fahrzeug,
                                  ultimateAktiv, schildAktiv, g_strecke)

                For s = spielfigur_spalte To spielfigur_spalte + fahrzeugBreite - 1
                    If s >= 0 And s <= SPALTE_MAX Then
                        Dim kollisionsZelle As Char = spielfeld(KOLLISIONS_ZEILE, s)

                        If kollisionsZelle = ITEM_LINKS Or
                           kollisionsZelle = ITEM_MITTE Or
                           kollisionsZelle = ITEM_RECHTS Then

                            Randomize()
                            Dim powerUp As Integer = CInt(Math.Floor(VBMath.Rnd() * 4))


                            ' Statistik: ein Item eingesammelt

                            g_statItems += 1

                            Select Case powerUp
                                Case ITEM_TYP_LEBEN
                                    leben = Math.Min(leben + 1, 9)
                                    g_itemMeldung = "+LEBEN"
                                    g_itemMeldungTicks = 15
                                Case ITEM_TYP_SCHILD
                                    schildAktiv = True
                                    schildEnde = DateTime.Now.AddSeconds(SCHILD_DAUER_SEKUNDEN)
                                    g_itemMeldung = "+SCHILD"
                                    g_itemMeldungTicks = 15
                                Case ITEM_TYP_ULTIMATE
                                    ultimateVerfuegbar = True
                                    g_itemMeldung = "+ULTIMATE"
                                    g_itemMeldungTicks = 15
                                Case ITEM_TYP_SCHUSS
                                    schussMunition += SCHUSS_PRO_ITEM
                                    g_itemMeldung = "+SCHUSS"
                                    g_itemMeldungTicks = 15
                            End Select

                            Dim boxStart As Integer = s
                            Do While boxStart > aktLeitLinks And
                                     spielfeld(KOLLISIONS_ZEILE, boxStart) <> ITEM_LINKS
                                boxStart -= 1
                            Loop
                            For b As Integer = boxStart To Math.Min(boxStart + 2, SPALTE_MAX)
                                spielfeld(KOLLISIONS_ZEILE, b) = " "c
                            Next
                            Exit For
                        End If

                        If kollisionsZelle = HINDERNIS_ZEICHEN Then
                            If Not ultimateAktiv And Not schildAktiv And hindernisSchutz = 0 Then
                                leben -= 1
                                hindernisSchutz = BEWEGUNG_SPIELFIGUR


                                ' Statistik: ein Hindernis getroffen

                                g_statHindernisse += 1
                            End If
                            spielfeld(KOLLISIONS_ZEILE, s) = " "c
                        End If

                        If kollisionsZelle = BOOST_ZEICHEN Then


                            ' Boost-Feld überfahren -> für 2 Sekunden schneller

                            boostEnde = DateTime.Now.AddSeconds(BOOST_DAUER_SEKUNDEN)
                            g_itemMeldung = "BOOST!"
                            g_itemMeldungTicks = 15


                            ' Das ganze Boost-Feld aus dem Spielfeld entfernen.
                            ' Erst die linke Kante suchen, dann alle ">" löschen.

                            Dim boostLinks As Integer = s
                            Do While boostLinks > 0 AndAlso
                                     spielfeld(KOLLISIONS_ZEILE, boostLinks - 1) = BOOST_ZEICHEN
                                boostLinks -= 1
                            Loop
                            Do While boostLinks <= SPALTE_MAX AndAlso
                                     spielfeld(KOLLISIONS_ZEILE, boostLinks) = BOOST_ZEICHEN
                                spielfeld(KOLLISIONS_ZEILE, boostLinks) = " "c
                                boostLinks += 1
                            Loop
                        End If
                    End If
                Next

                ' Schild ist für eingestellte Dauer aktiv
                schildAktiv = (DateTime.Now < schildEnde)
                If ultimateAktiv Then
                    ultimateTicks -= 1
                    If ultimateTicks <= 0 Then ultimateAktiv = False
                End If

                Console.SetCursorPosition(0, ZEILE_MAX)
                Dim statusSchild As String = If(schildAktiv, "[SCHILD] ", "         ")
                Dim statusUlt As String = If(ultimateAktiv, "[ULT!]    ",
                                             If(ultimateVerfuegbar, "[U:bereit]", "          "))
                Dim fortschritt As Integer = CInt((1.0 - segRestlaenge / CDbl(segLaenge)) * 10)
                Dim balken As String = "[" & New String("="c, fortschritt) &
                                       New String("-"c, 10 - fortschritt) & "]"
                Dim stufeAnzeige As String = "St:" & schwierigkeitStufe.ToString().PadLeft(2)
                Dim meldungAnzeige As String = If(g_itemMeldung <> "",
                    g_itemMeldung.PadRight(10), New String(" "c, 10))

                Console.Write("Leben:" & leben &
                              " Meter:" & meter.ToString().PadLeft(5) &
                              " " & stufeAnzeige &
                              " " & balken &
                              " " & statusSchild & statusUlt &
                              " Mun:" & schussMunition)

                If g_itemMeldung <> "" Then
                    Console.ForegroundColor = ConsoleColor.Yellow
                    Console.Write(" " & meldungAnzeige)
                    Strecke_Farben_Setzen(g_strecke)
                Else
                    Console.Write(New String(" "c, 11))
                End If

                Strecke_Farben_Setzen(g_strecke)


                ' Wartezeit pro Schritt – während der Zeitlupe (Vollbremsung) läuft das Spiel langsamer, während eines Boosts schneller.

                Dim aktSleep As Integer = CInt(wartezeit / BEWEGUNG_SPIELFIGUR)
                If slowmoTicks > 0 Then
                    aktSleep = aktSleep * ULT_SLOWMO_FAKTOR
                    slowmoTicks -= 1
                End If
                If DateTime.Now < boostEnde Then
                    aktSleep = aktSleep \ BOOST_FAKTOR
                End If
                Threading.Thread.Sleep(aktSleep)
            Next

            Do : taste = Tastatur_Abfrage() : Loop Until taste = NO_KEY
            wartezeit = wartezeit * 0.99
            If wartezeit < 0 Then wartezeit = 0

        Loop Until leben <= 0

        Game_Over(meter)
    End Sub

#End Region

#Region "MenuesUndIntro"

    ' Menüs
    ' Ki-Entwurf - von uns integriert sowie spezifisch angepasst für das Spiel.
    Sub Menue_Fahrzeug()
        ' Musik für dieses Menü
        Musik_Starten("Fahrzeugauswahl.wav")
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Clear()
        Zentriert_Schreiben("=== FAHRZEUG WAEHLEN ===", 2)
        Console.SetCursorPosition(20, 4) : Console.WriteLine("  1. Standard – Ultimate: Vollbremsung")
        Console.ForegroundColor = ConsoleColor.White
        Console.SetCursorPosition(25, 5) : Console.WriteLine(Fahrzeug_Zeile1(1))
        Console.SetCursorPosition(25, 6) : Console.WriteLine(Fahrzeug_Zeile2(1))
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.SetCursorPosition(20, 8) : Console.WriteLine("  2. Buggy  – Ultimate: 5 Sek. unverwundbar")
        Console.ForegroundColor = ConsoleColor.Green
        Console.SetCursorPosition(25, 9) : Console.WriteLine(Fahrzeug_Zeile1(2))
        Console.SetCursorPosition(25, 10) : Console.WriteLine(Fahrzeug_Zeile2(2))
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.SetCursorPosition(20, 12) : Console.WriteLine("  3. Klein  – Ultimate: Doppelsprung")
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.SetCursorPosition(25, 13) : Console.WriteLine(Fahrzeug_Zeile1(3))
        Console.SetCursorPosition(25, 14) : Console.WriteLine(Fahrzeug_Zeile2(3))
        Console.SetCursorPosition(20, 16)
        If g_goldFreigeschaltet Then
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.WriteLine("  4. Gold   [FREIGESCHALTET] – Ultimate: Goldraeumung")
            Console.SetCursorPosition(25, 17) : Console.WriteLine(Fahrzeug_Zeile1(4))
            Console.SetCursorPosition(25, 18) : Console.WriteLine(Fahrzeug_Zeile2(4))
        Else
            Console.ForegroundColor = ConsoleColor.DarkGray
            Console.WriteLine("  4. Gold   – ab " & GOLD_UNLOCK_METER & " Metern freischaltbar")
        End If
        Console.ForegroundColor = ConsoleColor.Cyan
        Zentriert_Schreiben("Aktuell: " & Fahrzeug_Name(g_fahrzeug), 20)
        Console.SetCursorPosition(20, 21) : Console.Write("  Eingabe (1-" & If(g_goldFreigeschaltet, "4", "3") & "): ")
        Console.CursorVisible = True
        Dim eingabe As String = Console.ReadLine()
        Console.CursorVisible = False
        Dim wahl As Integer = 0
        If Integer.TryParse(eingabe, wahl) Then
            If wahl >= 1 And wahl <= 3 Then
                g_fahrzeug = wahl
            ElseIf wahl = 4 And g_goldFreigeschaltet Then
                g_fahrzeug = 4
            End If
        End If
        Musik_Stoppen()
    End Sub

    Sub Menue_CCM()
        Musik_Starten("CCM-Stufe_wählen.wav")   ' Auswahlmusik
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Green
        Console.Clear()
        Zentriert_Schreiben("=== CCM-STUFE WAEHLEN ===", 2)
        Console.SetCursorPosition(20, 4) : Console.WriteLine("   50 CCM  Langsam   (Einsteiger)")
        Console.SetCursorPosition(20, 5) : Console.WriteLine("  100 CCM  Mittel    (Normal)")
        Console.SetCursorPosition(20, 6) : Console.WriteLine("  150 CCM  Schnell   (Profi)")
        Zentriert_Schreiben("Aktuell: " & g_ccm & " CCM", 8)
        Console.SetCursorPosition(20, 9) : Console.Write("  Eingabe (50 / 100 / 150): ")
        Console.CursorVisible = True
        Dim eingabe As String = Console.ReadLine()
        Console.CursorVisible = False
        Dim wahl As Integer = 0
        If Integer.TryParse(eingabe, wahl) Then
            If wahl = 50 Or wahl = 100 Or wahl = 150 Then g_ccm = wahl
        End If
        Musik_Stoppen()
    End Sub

    Sub Menue_Strecke()
        Musik_Starten("Streckenauswahl.wav")   ' Strecken-Musik
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Magenta
        Console.Clear()
        Zentriert_Schreiben("=== STRECKE WAEHLEN ===", 2)
        Console.SetCursorPosition(20, 4)
        Console.ForegroundColor = ConsoleColor.Cyan : Console.WriteLine("  1. Eisstrecke  Weiss / Cyan")
        Console.SetCursorPosition(20, 5)
        Console.ForegroundColor = ConsoleColor.Yellow : Console.WriteLine("  2. Wueste      Gelb / Schwarz")
        Console.SetCursorPosition(20, 6)
        Console.ForegroundColor = ConsoleColor.White : Console.WriteLine("  3. Autobahn    Grau / Weiss")
        Console.ForegroundColor = ConsoleColor.Magenta
        Zentriert_Schreiben("Aktuell: " & Strecke_Name(g_strecke), 8)
        Console.SetCursorPosition(20, 9) : Console.Write("  Eingabe (1 / 2 / 3): ")
        Console.CursorVisible = True
        Dim eingabe As String = Console.ReadLine()
        Console.CursorVisible = False
        Dim wahl As Integer = 0
        If Integer.TryParse(eingabe, wahl) Then
            If wahl >= 1 And wahl <= 3 Then g_strecke = wahl
        End If
        Musik_Stoppen()
    End Sub


    ' Optionen (Schleife bis Zurück)
    ' Fehlerbehebng durch Ki-Einsatz (Anpassung der Musikstruktur sowie der Spielmechanik).
    Sub Menue_Optionen()
        Dim fertig As Boolean = False
        Do
            Console.BackgroundColor = ConsoleColor.Black
            Console.ForegroundColor = ConsoleColor.White
            Console.Clear()
            Zentriert_Schreiben("=== OPTIONEN ===", 2)

            Console.SetCursorPosition(20, 5)
            Console.WriteLine("  1. Musik          : " & If(g_musikAn, "AN", "AUS"))
            Console.SetCursorPosition(20, 6)
            Console.WriteLine("  2. Startleben     : " & g_startLeben)
            Console.SetCursorPosition(20, 7)
            Console.WriteLine("  3. Statistik zuruecksetzen")
            Console.SetCursorPosition(20, 8)
            Console.WriteLine("  4. Zurueck")

            Console.ForegroundColor = ConsoleColor.Gray
            Console.SetCursorPosition(20, 11) : Console.Write("  Eingabe (1-4): ")
            Console.ForegroundColor = ConsoleColor.White
            Console.CursorVisible = True
            Dim eingabe As String = Console.ReadLine()
            Console.CursorVisible = False

            Select Case eingabe
                Case "1"


                    ' Musik an/aus schalten

                    g_musikAn = Not g_musikAn
                    If g_musikAn Then


                        ' Wieder eingeschaltet -> Menümusik sofort starten

                        Musik_Starten("Startbildschirm.wav")
                    Else
                        Musik_Stoppen()
                    End If
                Case "2"


                    ' Startleben durchschalten 3 -> 5 -> 7 -> 3

                    Select Case g_startLeben
                        Case 3 : g_startLeben = 5
                        Case 5 : g_startLeben = 7
                        Case Else : g_startLeben = 3
                    End Select
                Case "3"


                    ' Statistik zurücksetzen (mit Sicherheitsabfrage)

                    Console.SetCursorPosition(20, 13)
                    Console.ForegroundColor = ConsoleColor.Red
                    Console.Write("  Wirklich zuruecksetzen? (j/n): ")
                    Console.ForegroundColor = ConsoleColor.White
                    Console.CursorVisible = True
                    Dim antwort As String = Console.ReadLine()
                    Console.CursorVisible = False
                    If antwort IsNot Nothing AndAlso antwort.Trim().ToLower() = "j" Then
                        Statistik_Zuruecksetzen()
                    End If
                Case "4"
                    fertig = True
            End Select
        Loop Until fertig
    End Sub

    Sub Menue_Anleitung()
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White
        Console.Clear()
        Zentriert_Schreiben("=== ANLEITUNG ===", 1)
        Zentriert_Schreiben("Ziel: Fahre so weit wie moeglich!", 3)
        Console.SetCursorPosition(5, 4) : Console.WriteLine("Steuerung: PFEIL LINKS / RECHTS zum Ausweichen.")
        Console.SetCursorPosition(5, 5) : Console.WriteLine("Schiessen: PFEIL HOCH (mit Munition) zerstoert ein Hindernis.")
        Console.SetCursorPosition(5, 6) : Console.WriteLine("Ultimate:  LEERTASTE wenn [U:bereit] sichtbar.")
        Console.SetCursorPosition(5, 7) : Console.WriteLine("Pause:     Taste P pausiert das Spiel.")
        Console.SetCursorPosition(5, 8) : Console.WriteLine("Stufe:     Alle 100m steigt die Schwierigkeit.")
        Console.SetCursorPosition(5, 9)
        Console.ForegroundColor = ConsoleColor.DarkYellow
        Console.Write("  [?]")
        Console.ForegroundColor = ConsoleColor.White
        Console.WriteLine("  Item-Box – zufaelliger Power-Up:")
        Console.SetCursorPosition(5, 10)
        Console.ForegroundColor = ConsoleColor.Yellow : Console.WriteLine("       +LEBEN    Extra Leben")
        Console.SetCursorPosition(5, 11)
        Console.ForegroundColor = ConsoleColor.Cyan : Console.WriteLine("       +SCHILD   Schutzschild")
        Console.SetCursorPosition(5, 12)
        Console.ForegroundColor = ConsoleColor.Magenta : Console.WriteLine("       +ULTIMATE Ultimate aufladen")
        Console.SetCursorPosition(5, 13)
        Console.ForegroundColor = ConsoleColor.Red : Console.WriteLine("       +SCHUSS   " & SCHUSS_PRO_ITEM & " Schuss Munition")
        Console.SetCursorPosition(5, 14)
        Console.ForegroundColor = ConsoleColor.Red
        Console.Write("  " & HINDERNIS_ZEICHEN & HINDERNIS_ZEICHEN)
        Console.ForegroundColor = ConsoleColor.White
        Console.WriteLine("  Hindernis – ausweichen oder abschiessen!")
        Console.SetCursorPosition(5, 15)
        Console.ForegroundColor = ConsoleColor.Yellow : Console.WriteLine("Fahrzeuge:")
        Console.SetCursorPosition(5, 16) : Console.ForegroundColor = ConsoleColor.White
        Console.WriteLine("  /##\  Standard – Vollbremsung (langsam + unverwundbar)")
        Console.SetCursorPosition(5, 17) : Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("  [BB]  Buggy    – 5 Sek. unverwundbar")
        Console.SetCursorPosition(5, 18) : Console.ForegroundColor = ConsoleColor.Cyan
        Console.WriteLine("  (oo)  Klein    – Doppelsprung")
        Console.SetCursorPosition(5, 19) : Console.ForegroundColor = ConsoleColor.Yellow
        Console.WriteLine("  =$=$  Gold     – Goldraeumung (ab " & GOLD_UNLOCK_METER & " m)")
        Console.SetCursorPosition(5, 20) : Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("  >>  Boost-Feld – " & BOOST_DAUER_SEKUNDEN & " Sek. schneller fahren")
        Console.SetCursorPosition(5, 22)
        Console.ForegroundColor = ConsoleColor.Gray
        Zentriert_Schreiben("[ENTER] Zurueck", 22)
        Console.CursorVisible = True
        Console.ReadLine()
        Console.CursorVisible = False
    End Sub

    ' Intro beim Programmstart
    ' Ki-Entwurf - von uns integriert sowie spezifisch angepasst für das Spiel.
    Sub Intro_Animation()
        Console.CursorVisible = False
        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()


        Musik_Starten("Startbildschirm.wav")

        ' Kart von oben (3 Zeilen)

        Dim kart0 As String = " /==\ "
        Dim kart1 As String = "[|oo|]"
        Dim kart2 As String = " \==/ "
        Dim kartBreite As Integer = 6
        Dim kartZeile As Integer = 13

        Dim hinweis As String = "Druecke eine beliebige Taste zum Starten"

        Dim kartSpalte As Integer = 0
        Dim prevSpalte As Integer = 0
        Dim frame As Integer = 0

        Do

            ' Karierte Flaggen oben und unten

            Console.SetCursorPosition(0, 0)
            For sc As Integer = 0 To SPALTE_MAX - 1
                Console.BackgroundColor = If((sc + frame) Mod 2 = 0,
                                             ConsoleColor.White, ConsoleColor.Black)
                Console.Write(" "c)
            Next
            Console.SetCursorPosition(0, ZEILE_MAX)
            For sc As Integer = 0 To SPALTE_MAX - 1
                Console.BackgroundColor = If((sc + frame) Mod 2 = 0,
                                             ConsoleColor.White, ConsoleColor.Black)
                Console.Write(" "c)
            Next
            Console.BackgroundColor = ConsoleColor.Black


            ' Titel – "MARIO KART" pulsiert zwischen Rot und Gelb

            Console.ForegroundColor = ConsoleColor.Cyan
            Zentriert_Schreiben("S P A C E   I N V A D E R S", 5)
            Console.ForegroundColor = ConsoleColor.White
            Zentriert_Schreiben("x", 6)
            Console.ForegroundColor = If((frame \ 3) Mod 2 = 0,
                                         ConsoleColor.Red, ConsoleColor.Yellow)
            Zentriert_Schreiben("M A R I O   K A R T", 7)


            ' Kart an alter Position löschen

            Dim leer As String = New String(" "c, kartBreite)
            Console.SetCursorPosition(prevSpalte, kartZeile) : Console.Write(leer)
            Console.SetCursorPosition(prevSpalte, kartZeile + 1) : Console.Write(leer)
            Console.SetCursorPosition(prevSpalte, kartZeile + 2) : Console.Write(leer)


            ' Kart an neuer Position zeichnen

            Console.ForegroundColor = ConsoleColor.Red
            Console.SetCursorPosition(kartSpalte, kartZeile) : Console.Write(kart0)
            Console.SetCursorPosition(kartSpalte, kartZeile + 1) : Console.Write(kart1)
            Console.SetCursorPosition(kartSpalte, kartZeile + 2) : Console.Write(kart2)


            ' Blinkender Hinweis

            If (frame \ 4) Mod 2 = 0 Then
                Console.ForegroundColor = ConsoleColor.White
                Zentriert_Schreiben(hinweis, 20)
            Else
                Zentriert_Schreiben(New String(" "c, hinweis.Length), 20)
            End If


            ' Beliebige Taste gedrueckt? -> Intro beenden

            If Console.KeyAvailable Then
                Console.ReadKey(True)
                Exit Do
            End If


            ' Kart weiterbewegen (am rechten Rand wieder von links)

            prevSpalte = kartSpalte
            kartSpalte += 2
            If kartSpalte > SPALTE_MAX - kartBreite Then kartSpalte = 0
            frame += 1
            Threading.Thread.Sleep(80)
        Loop

        Musik_Stoppen()
        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()
    End Sub

    ' Hauptmenü (Endlosschleife bis Beenden)
    ' Fehlerbehebung durch Ki-Einsatz (Design- und Logik-Optimierung, um reibungslose Navigation zu gewährleisten).
    Sub Hauptmenue()
        Dim wahl As String
        Musik_Starten("Startbildschirm.wav")

        Do
            Console.BackgroundColor = ConsoleColor.Black
            Console.Clear()

            Console.ForegroundColor = ConsoleColor.DarkGray
            Zentriert_Schreiben(New String("*", 53), 1)
            Console.ForegroundColor = ConsoleColor.Cyan
            Zentriert_Schreiben("S P A C E   I N V A D E R S", 2)
            Console.ForegroundColor = ConsoleColor.Yellow
            Zentriert_Schreiben("- - - - - x - - - - -", 3)
            Console.ForegroundColor = ConsoleColor.Red
            Zentriert_Schreiben("M A R I O   K A R T", 4)
            Console.ForegroundColor = ConsoleColor.DarkGray
            Zentriert_Schreiben(New String("*", 53), 5)

            Console.ForegroundColor = ConsoleColor.White
            Zentriert_Schreiben("Fahrzeug : " & Fahrzeug_Name(g_fahrzeug) &
                                 "  |  Strecke : " & Strecke_Name(g_strecke) &
                                 "  |  CCM : " & g_ccm, 6)
            Console.ForegroundColor = ConsoleColor.Yellow
            Zentriert_Schreiben("Ultimate : " & Fahrzeug_Ultimate_Info(g_fahrzeug), 7)

            Console.ForegroundColor = ConsoleColor.DarkGray
            Zentriert_Schreiben(New String("-", 45), 9)

            Console.ForegroundColor = ConsoleColor.Red
            Zentriert_Schreiben("[1]  Spiel starten", 10)
            Console.ForegroundColor = ConsoleColor.Green
            Zentriert_Schreiben("[2]  Fahrzeug waehlen", 11)
            Console.ForegroundColor = ConsoleColor.Blue
            Zentriert_Schreiben("[3]  CCM-Stufe waehlen", 12)
            Console.ForegroundColor = ConsoleColor.Yellow
            Zentriert_Schreiben("[4]  Strecke waehlen", 13)
            Console.ForegroundColor = ConsoleColor.Cyan
            Zentriert_Schreiben("[5]  Highscore anzeigen", 14)
            Console.ForegroundColor = ConsoleColor.White
            Zentriert_Schreiben("[6]  Statistik", 15)
            Console.ForegroundColor = ConsoleColor.Magenta
            Zentriert_Schreiben("[7]  Achievements", 16)
            Console.ForegroundColor = ConsoleColor.Green
            Zentriert_Schreiben("[8]  Optionen", 17)
            Console.ForegroundColor = ConsoleColor.White
            Zentriert_Schreiben("[9]  Anleitung", 18)
            Console.ForegroundColor = ConsoleColor.DarkRed
            Zentriert_Schreiben("[10] Beenden", 19)

            Console.ForegroundColor = ConsoleColor.DarkGray
            Zentriert_Schreiben(New String("-", 45), 20)

            Dim eingabeText As String = "Eingabe: "
            Dim eingabeSpalte As Integer = (SPALTE_MAX \ 2) - (eingabeText.Length \ 2)
            Console.ForegroundColor = ConsoleColor.Gray
            Console.SetCursorPosition(eingabeSpalte, 21)
            Console.Write(eingabeText)
            Console.CursorVisible = True
            wahl = Console.ReadLine()
            Console.CursorVisible = False

            Select Case wahl
                Case "1"
                    Musik_Stoppen()
                    Spielablauf()
                    Musik_Starten("Startbildschirm.wav")
                Case "2" : Menue_Fahrzeug() : Musik_Starten("Startbildschirm.wav")
                Case "3" : Menue_CCM() : Musik_Starten("Startbildschirm.wav")
                Case "4" : Menue_Strecke() : Musik_Starten("Startbildschirm.wav")
                Case "5" : Highscore_Anzeigen() : Musik_Starten("Startbildschirm.wav")
                Case "6" : Statistik_Anzeigen()
                Case "7" : Achievements_Anzeigen()
                Case "8" : Menue_Optionen() : Musik_Starten("Startbildschirm.wav")
                Case "9" : Menue_Anleitung()
                Case "10" : Exit Do
            End Select
        Loop

        Musik_Stoppen()
    End Sub

    ' Programmstart

    Sub Main()
        Console.CursorVisible = False
        Intro_Animation()
        Hauptmenue()
    End Sub

#End Region

End Module