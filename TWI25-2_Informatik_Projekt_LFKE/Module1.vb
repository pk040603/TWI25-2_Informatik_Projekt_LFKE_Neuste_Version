Module Module1


    '  KONSTANTEN – Tastatur

    Const NO_KEY = 0
    Const CURSOR_LEFT = 1
    Const CURSOR_RIGHT = 2
    Const UNKNOWN_KEY = 99


    '  KONSTANTEN – Spielfeld

    Const SPALTE_MAX = 79
    Const ZEILE_MAX = 24
    Const BEWEGUNG_SPIELFIGUR = 10
    Const KOLLISIONS_ZEILE = ZEILE_MAX - 2


    '  KONSTANTEN – Leitplanken

    Const LEITPLANKE_ZEICHEN As Char = "|"c

    Const STRASSE_BREITE_START As Double = 42.0
    Const STRASSE_BREITE_MIN As Double = 8.0
    Const STRASSE_BREITE_MAX As Double = 46.0
    Const STRASSE_MITTE_STANDARD As Double = SPALTE_MAX / 2


    '  KONSTANTEN – Streckengenerator

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


    '  KONSTANTEN – CCM

    Const CCM_50_WARTEZEIT = 300
    Const CCM_100_WARTEZEIT = 200
    Const CCM_150_WARTEZEIT = 120


    '  KONSTANTEN – Items

    Const ITEM_LINKS As Char = "["c
    Const ITEM_MITTE As Char = "?"c
    Const ITEM_RECHTS As Char = "]"c
    Const ITEM_CHANCE = 12

    Const ITEM_TYP_LEBEN = 0
    Const ITEM_TYP_SCHILD = 1
    Const ITEM_TYP_ULTIMATE = 2


    '  KONSTANTEN – Hindernisse

    Const HINDERNIS_ZEICHEN As Char = "█"c
    Const HINDERNIS_CHANCE_START = 6
    Const HINDERNIS_CHANCE_MIN = 3
    Const SCHWIERIGKEIT_INTERVALL = 100


    '  KONSTANTEN – Strecken

    Const STRECKE_EIS = 1
    Const STRECKE_WUESTE = 2
    Const STRECKE_AUTOBAHN = 3


    '  KONSTANTEN – Freischalt-Schwelle

    Const GOLD_UNLOCK_METER = 400


    '  GLOBALE SPIELVARIABLEN

    Dim g_ccm As Integer = 100
    Dim g_strecke As Integer = STRECKE_AUTOBAHN
    Dim g_fahrzeug As Integer = 1
    Dim g_goldFreigeschaltet As Boolean = False
    Dim g_itemMeldung As String = ""
    Dim g_itemMeldungTicks As Integer = 0

    Dim g_musikPlayer As New System.Media.SoundPlayer()


    '  HIGHSCORE

    Structure HighscoreEintrag
        Dim name As String
        Dim meter As Integer
    End Structure
    Dim g_highscores(4) As HighscoreEintrag
    Dim g_highscoreAnzahl As Integer = 0


    '  TASTATUR

    Function Tastatur_Abfrage() As Integer
        If Console.KeyAvailable = False Then Return NO_KEY
        Dim cki As ConsoleKeyInfo = Console.ReadKey(True)
        If cki.Key = ConsoleKey.LeftArrow Then Return CURSOR_LEFT
        If cki.Key = ConsoleKey.RightArrow Then Return CURSOR_RIGHT
        Return UNKNOWN_KEY
    End Function


    '  MUSIK

    Sub Musik_Starten(ByVal dateiname As String)
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


    '  HILFSFUNKTION – Text zentriert ausgeben

    Sub Zentriert_Schreiben(ByVal text As String, ByVal zeile As Integer)
        Dim spalte As Integer = (SPALTE_MAX \ 2) - (text.Length \ 2)
        If spalte < 0 Then spalte = 0
        Console.SetCursorPosition(spalte, zeile)
        Console.Write(text)
    End Sub


    '  STARTAUFSTELLUNG MIT AMPEL

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


    '  STRECKEN-HILFSFUNKTIONEN

    Function Strecke_Name(ByVal id As Integer) As String
        Select Case id
            Case STRECKE_EIS : Return "Eisstrecke"
            Case STRECKE_WUESTE : Return "Wueste    "
            Case STRECKE_AUTOBAHN : Return "Autobahn  "
            Case Else : Return "Unbekannt "
        End Select
    End Function

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


    '  FAHRZEUG – 2x2 ASCII ART

    Function Fahrzeug_Zeile1(ByVal id As Integer) As String
        Select Case id
            Case 1 : Return "/##\"
            Case 2 : Return "[BB]"
            Case 3 : Return " /\ "
            Case 4 : Return "-$$-"
            Case Else : Return "/##\"
        End Select
    End Function

    Function Fahrzeug_Zeile2(ByVal id As Integer) As String
        Select Case id
            Case 1 : Return "\##/"
            Case 2 : Return "|BB|"
            Case 3 : Return "(oo)"
            Case 4 : Return "=$=$"
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
            Case 1 : Return "Kein Ultimate"
            Case 2 : Return "5 Sek. unverwundbar [LEERTASTE]"
            Case 3 : Return "Doppelsprung 2 Felder [LEERTASTE]"
            Case 4 : Return "Kein Ultimate"
            Case Else : Return ""
        End Select
    End Function

    Sub Fahrzeug_Zeichnen(ByVal spalte As Integer,
                          ByVal id As Integer,
                          ByVal ultimateAktiv As Boolean,
                          ByVal schildAktiv As Boolean,
                          ByVal strecke As Integer)
        Dim farbe As ConsoleColor = Fahrzeug_Farbe(id)
        If ultimateAktiv Then farbe = ConsoleColor.Yellow
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


    '  STARTPOSITION BERECHNEN

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


    '  STRECKENGENERATOR

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


    '  ZEILE ERZEUGEN
    '  nurStrasse = True  ->  nur Leitplanken, KEINE Hindernisse
    '                          und KEINE Items (fuer Spielstart)

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

            ' Nur Strasse mit Leitplanken – keine Hindernisse, keine Items.
            ' (Leitplanken sind oben bereits gesetzt)
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
        End If
    End Sub


    '  ULTIMATE

    Function Ultimate_Ausfuehren(ByVal fahrzeug As Integer,
                                  ByRef spielfigur_spalte As Integer) As Integer
        Select Case fahrzeug
            Case 2 : Return 25
            Case 3
                spielfigur_spalte = Math.Max(0, spielfigur_spalte - 2)
                Return 0
            Case Else : Return 0
        End Select
    End Function


    '  HIGHSCORE

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
        Musik_Starten("Highscore.wav")
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Yellow
        Console.Clear()
        Console.SetCursorPosition(20, 3) : Console.WriteLine("=== HIGHSCORE ===")
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
        Console.SetCursorPosition(20, 15)
        Console.ForegroundColor = ConsoleColor.Gray
        Console.WriteLine("  [ENTER] Zurueck")
        Console.CursorVisible = True : Console.ReadLine() : Console.CursorVisible = False
        Musik_Stoppen()
    End Sub


    '  GAME OVER

    Sub Game_Over(ByVal meter As Integer)
        Musik_Stoppen()
        Sound_GameOver()

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
        Console.CursorVisible = True : Console.ReadLine() : Console.CursorVisible = False
    End Sub


    '  HAUPTSPIELSCHLEIFE

    Sub Spielablauf()
        Dim leben As Integer = 5
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
        Dim schildTicks As Integer = 0
        Dim ultimateAktiv As Boolean = False
        Dim ultimateTicks As Integer = 0
        Dim ultimateVerfuegbar As Boolean = False
        Dim hindernisSchutz As Integer = 0

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


        ' Spielfeld vorbelegen:
        ' Nur Strasse mit Leitplanken (nurStrasse = True) -> keine
        ' Hindernisse und keine Items am Start. Die Strecke scrollt
        ' von oben rein. Startmarkierungen P1/P2/P3 unten.

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

        Dim wartezeit As Single
        Select Case g_ccm
            Case 50 : wartezeit = CCM_50_WARTEZEIT
            Case 150 : wartezeit = CCM_150_WARTEZEIT
            Case Else : wartezeit = CCM_100_WARTEZEIT
        End Select

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
            Erzeuge_Zeile(zeile, aktBreite, aktMitte, g_strecke, hindernisChance, 0, False)

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

            Console.SetCursorPosition(0, 0)
            For z = 0 To ZEILE_MAX - 3
                For s = 0 To SPALTE_MAX
                    Dim zelle As Char = spielfeld(z, s)
                    Select Case zelle
                        Case LEITPLANKE_ZEICHEN
                            Curb_Farbe_Setzen((curbPhase + z) Mod 2)
                            Console.Write(zelle)
                            Strecke_Farben_Setzen(g_strecke)
                        Case ITEM_LINKS, ITEM_MITTE, ITEM_RECHTS
                            Console.BackgroundColor = ConsoleColor.DarkYellow
                            Console.ForegroundColor = ConsoleColor.White
                            Console.Write(zelle)
                            Strecke_Farben_Setzen(g_strecke)
                        Case HINDERNIS_ZEICHEN
                            Console.ForegroundColor = ConsoleColor.Red
                            Console.Write(zelle)
                            Strecke_Farben_Setzen(g_strecke)
                        Case "P"c
                            Console.ForegroundColor = ConsoleColor.Yellow
                            Console.Write(zelle)
                            Strecke_Farben_Setzen(g_strecke)
                        Case "1"c
                            Console.ForegroundColor = ConsoleColor.Red
                            Console.Write(zelle)
                            Strecke_Farben_Setzen(g_strecke)
                        Case "2"c
                            Console.ForegroundColor = ConsoleColor.Yellow
                            Console.Write(zelle)
                            Strecke_Farben_Setzen(g_strecke)
                        Case "3"c
                            Console.ForegroundColor = ConsoleColor.Green
                            Console.Write(zelle)
                            Strecke_Farben_Setzen(g_strecke)
                        Case Else
                            Console.Write(zelle)
                    End Select
                Next
                Console.WriteLine()
            Next

            Dim leitplankeGetroffen As Boolean = False

            For i = 1 To BEWEGUNG_SPIELFIGUR
                taste = NO_KEY
                If Console.KeyAvailable Then
                    Dim cki As ConsoleKeyInfo = Console.ReadKey(True)
                    If cki.Key = ConsoleKey.LeftArrow Then
                        taste = CURSOR_LEFT
                    ElseIf cki.Key = ConsoleKey.RightArrow Then
                        taste = CURSOR_RIGHT
                    ElseIf cki.Key = ConsoleKey.Spacebar Then
                        If ultimateVerfuegbar And Not ultimateAktiv Then
                            Dim ticks As Integer = Ultimate_Ausfuehren(g_fahrzeug, spielfigur_spalte)
                            If ticks > 0 Then
                                ultimateAktiv = True
                                ultimateTicks = ticks
                            End If
                            ultimateVerfuegbar = False
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

                Fahrzeug_Loeschen(spielfigur_spalte, g_fahrzeug)

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
                            Dim powerUp As Integer = CInt(Math.Floor(VBMath.Rnd() * 3))

                            Select Case powerUp
                                Case ITEM_TYP_LEBEN
                                    leben = Math.Min(leben + 1, 9)
                                    g_itemMeldung = "+LEBEN"
                                    g_itemMeldungTicks = 15
                                Case ITEM_TYP_SCHILD
                                    schildAktiv = True
                                    schildTicks = 15
                                    g_itemMeldung = "+SCHILD"
                                    g_itemMeldungTicks = 15
                                Case ITEM_TYP_ULTIMATE
                                    ultimateVerfuegbar = True
                                    g_itemMeldung = "+ULTIMATE"
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
                            End If
                            spielfeld(KOLLISIONS_ZEILE, s) = " "c
                        End If
                    End If
                Next

                If schildAktiv Then
                    schildTicks -= 1
                    If schildTicks <= 0 Then schildAktiv = False
                End If
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
                Dim stufeAnzeige As String = "Stufe:" & schwierigkeitStufe.ToString().PadLeft(2)
                Dim meldungAnzeige As String = If(g_itemMeldung <> "",
                    g_itemMeldung.PadRight(10), New String(" "c, 10))

                Console.Write("Leben:" & leben &
                              "  Meter:" & meter.ToString().PadLeft(5) &
                              "  " & stufeAnzeige &
                              "  " & balken &
                              "  " & statusSchild & statusUlt)

                If g_itemMeldung <> "" Then
                    Console.ForegroundColor = ConsoleColor.Yellow
                    Console.Write(" " & meldungAnzeige)
                    Strecke_Farben_Setzen(g_strecke)
                Else
                    Console.Write(New String(" "c, 11))
                End If

                Strecke_Farben_Setzen(g_strecke)
                Threading.Thread.Sleep(CInt(wartezeit / BEWEGUNG_SPIELFIGUR))
            Next

            Do : taste = Tastatur_Abfrage() : Loop Until taste = NO_KEY
            wartezeit = wartezeit * 0.99
            If wartezeit < 0 Then wartezeit = 0

        Loop Until leben <= 0

        Game_Over(meter)
    End Sub


    '  MENUES

    Sub Menue_Fahrzeug()
        Musik_Starten("Fahrzeugauswahl.wav")
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Clear()
        Console.SetCursorPosition(20, 2) : Console.WriteLine("=== FAHRZEUG WAEHLEN ===")
        Console.SetCursorPosition(20, 4) : Console.WriteLine("  1. Standard")
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
            Console.WriteLine("  4. Gold   [FREIGESCHALTET]")
            Console.SetCursorPosition(25, 17) : Console.WriteLine(Fahrzeug_Zeile1(4))
            Console.SetCursorPosition(25, 18) : Console.WriteLine(Fahrzeug_Zeile2(4))
        Else
            Console.ForegroundColor = ConsoleColor.DarkGray
            Console.WriteLine("  4. Gold   – ab " & GOLD_UNLOCK_METER & " Metern freischaltbar")
        End If
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.SetCursorPosition(20, 20) : Console.WriteLine("  Aktuell: " & Fahrzeug_Name(g_fahrzeug))
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
        Musik_Starten("CCM-Stufe_wählen.wav")
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Green
        Console.Clear()
        Console.SetCursorPosition(20, 2) : Console.WriteLine("=== CCM-STUFE WAEHLEN ===")
        Console.SetCursorPosition(20, 4) : Console.WriteLine("   50 CCM  Langsam   (Einsteiger)")
        Console.SetCursorPosition(20, 5) : Console.WriteLine("  100 CCM  Mittel    (Normal)")
        Console.SetCursorPosition(20, 6) : Console.WriteLine("  150 CCM  Schnell   (Profi)")
        Console.SetCursorPosition(20, 8) : Console.WriteLine("  Aktuell: " & g_ccm & " CCM")
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
        Musik_Starten("Streckenauswahl.wav")
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Magenta
        Console.Clear()
        Console.SetCursorPosition(20, 2) : Console.WriteLine("=== STRECKE WAEHLEN ===")
        Console.SetCursorPosition(20, 4)
        Console.ForegroundColor = ConsoleColor.Cyan : Console.WriteLine("  1. Eisstrecke  Weiss / Cyan")
        Console.SetCursorPosition(20, 5)
        Console.ForegroundColor = ConsoleColor.Yellow : Console.WriteLine("  2. Wueste      Gelb / Schwarz")
        Console.SetCursorPosition(20, 6)
        Console.ForegroundColor = ConsoleColor.White : Console.WriteLine("  3. Autobahn    Grau / Weiss")
        Console.SetCursorPosition(20, 8)
        Console.ForegroundColor = ConsoleColor.Magenta : Console.WriteLine("  Aktuell: " & Strecke_Name(g_strecke))
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

    Sub Menue_Anleitung()
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White
        Console.Clear()
        Console.SetCursorPosition(15, 1) : Console.WriteLine("=== ANLEITUNG ===")
        Console.SetCursorPosition(5, 3) : Console.WriteLine("Ziel:      Fahre so weit wie moeglich!")
        Console.SetCursorPosition(5, 4) : Console.WriteLine("Steuerung: PFEIL LINKS / RECHTS zum Ausweichen.")
        Console.SetCursorPosition(5, 5) : Console.WriteLine("Strasse:   Wird breiter, enger und verschiebt sich.")
        Console.SetCursorPosition(5, 6) : Console.WriteLine("           Der Balken [====----] zeigt: naechste Kurve.")
        Console.SetCursorPosition(5, 7) : Console.WriteLine("Ultimate:  LEERTASTE wenn [U:bereit] sichtbar.")
        Console.SetCursorPosition(5, 8) : Console.WriteLine("Stufe:     Alle 100m steigt die Schwierigkeit.")
        Console.SetCursorPosition(5, 9)
        Console.ForegroundColor = ConsoleColor.DarkYellow
        Console.Write("  [?]")
        Console.ForegroundColor = ConsoleColor.White
        Console.WriteLine("  Item-Box – zufaelliger Power-Up!")
        Console.SetCursorPosition(5, 10)
        Console.ForegroundColor = ConsoleColor.Yellow : Console.WriteLine("       +LEBEN    Extra Leben")
        Console.SetCursorPosition(5, 11)
        Console.ForegroundColor = ConsoleColor.Cyan : Console.WriteLine("       +SCHILD   Schutzschild")
        Console.SetCursorPosition(5, 12)
        Console.ForegroundColor = ConsoleColor.Magenta : Console.WriteLine("       +ULTIMATE Ultimate aufladen")
        Console.SetCursorPosition(5, 13)
        Console.ForegroundColor = ConsoleColor.Red
        Console.Write("  " & HINDERNIS_ZEICHEN & HINDERNIS_ZEICHEN)
        Console.ForegroundColor = ConsoleColor.White
        Console.WriteLine("  Hindernis – ausweichen!")
        Console.SetCursorPosition(5, 14)
        Console.ForegroundColor = ConsoleColor.Yellow : Console.WriteLine("Fahrzeuge:")
        Console.SetCursorPosition(5, 15) : Console.ForegroundColor = ConsoleColor.White
        Console.WriteLine("  /##\  Standard – kein Ultimate")
        Console.SetCursorPosition(5, 16) : Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("  [BB]  Buggy    – 5 Sek. unverwundbar")
        Console.SetCursorPosition(5, 17) : Console.ForegroundColor = ConsoleColor.Cyan
        Console.WriteLine("  (oo)  Klein    – Doppelsprung")
        Console.SetCursorPosition(5, 18) : Console.ForegroundColor = ConsoleColor.Yellow
        Console.WriteLine("  =$=$  Gold     – ab " & GOLD_UNLOCK_METER & " Metern freischaltbar")
        Console.SetCursorPosition(5, 20)
        Console.ForegroundColor = ConsoleColor.Gray : Console.WriteLine("  [ENTER] Zurueck")
        Console.CursorVisible = True : Console.ReadLine() : Console.CursorVisible = False
    End Sub

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
                                 "  |  CCM : " & g_ccm, 7)
            Console.ForegroundColor = ConsoleColor.Yellow
            Zentriert_Schreiben("Ultimate : " & Fahrzeug_Ultimate_Info(g_fahrzeug), 8)

            Console.ForegroundColor = ConsoleColor.DarkGray
            Zentriert_Schreiben(New String("-", 45), 10)

            Console.ForegroundColor = ConsoleColor.Red
            Zentriert_Schreiben("[1]  Spiel starten", 11)
            Console.ForegroundColor = ConsoleColor.Green
            Zentriert_Schreiben("[2]  Fahrzeug waehlen", 12)
            Console.ForegroundColor = ConsoleColor.Blue
            Zentriert_Schreiben("[3]  CCM-Stufe waehlen", 13)
            Console.ForegroundColor = ConsoleColor.Yellow
            Zentriert_Schreiben("[4]  Strecke waehlen", 14)
            Console.ForegroundColor = ConsoleColor.Cyan
            Zentriert_Schreiben("[5]  Highscore anzeigen", 15)
            Console.ForegroundColor = ConsoleColor.White
            Zentriert_Schreiben("[6]  Anleitung", 16)
            Console.ForegroundColor = ConsoleColor.DarkRed
            Zentriert_Schreiben("[7]  Beenden", 17)

            Console.ForegroundColor = ConsoleColor.DarkGray
            Zentriert_Schreiben(New String("-", 45), 19)

            Dim eingabeText As String = "Eingabe: "
            Dim eingabeSpalte As Integer = (SPALTE_MAX \ 2) - (eingabeText.Length \ 2)
            Console.ForegroundColor = ConsoleColor.Gray
            Console.SetCursorPosition(eingabeSpalte, 20)
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
                Case "6" : Menue_Anleitung()
                Case "7" : Exit Do
            End Select
        Loop

        Musik_Stoppen()
    End Sub

    Sub Main()
        Console.CursorVisible = False
        Hauptmenue()
    End Sub

End Module