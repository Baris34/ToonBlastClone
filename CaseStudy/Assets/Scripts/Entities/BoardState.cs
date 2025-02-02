public enum BoardState
{
    Idle,       // Kullanýcý týklamasýný bekliyor
    Removing,   // Blok patlatma animasyonu
    Gravity,    // Bloklarýn düþüþü
    Refill,     // Eksik hücreleri doldurma
    Checking,   // BFS ile no moves? => Shuffle?
    Shuffling,  // Shuffle animasyonu
    Busy        // Geçici durum (opsiyonel)
}