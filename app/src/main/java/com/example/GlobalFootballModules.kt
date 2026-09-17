package com.example

import android.content.Context
import android.media.AudioAttributes
import android.media.MediaPlayer
import android.net.Uri
import java.io.File

// GESTIONNAIRE DE MUSIQUE PERSONNELLE
class CustomMusicManager(private val context: Context) {
    private var mediaPlayer: MediaPlayer? = null
    val playlistFilePaths = mutableListOf<String>()
    private var currentTrackIndex = 0

    fun addTrackToPlaylist(path: String) {
        val file = File(path)
        if (file.exists() && !playlistFilePaths.contains(path)) {
            playlistFilePaths.add(path)
        }
    }

    fun playNextTrack() {
        if (playlistFilePaths.isEmpty()) return
        currentTrackIndex = (currentTrackIndex + 1) % playlistFilePaths.size
        val filePath = playlistFilePaths[currentTrackIndex]

        mediaPlayer?.stop()
        mediaPlayer?.release()

        mediaPlayer = MediaPlayer().apply {
            setAudioAttributes(
                AudioAttributes.Builder()
                    .setContentType(AudioAttributes.CONTENT_TYPE_MUSIC)
                    .setUsage(AudioAttributes.USAGE_MEDIA)
                    .build()
            )
            setDataSource(context, Uri.fromFile(File(filePath)))
            prepare()
            start()
        }
    }
}

// CARTES ÉPIQUES ET JOUEURS
enum class CardRarity { BASE, HIGHLIGHT, EPIC, LEGENDARY }

data class PlayerCardData(
    val playerName: String,
    val overallRating: Int,
    val position: String,
    val rarity: CardRarity,
    val pace: Int,
    val shooting: Int,
    val passing: Int,
    val dribbling: Int,
    val defending: Int,
    val physical: Int
)
