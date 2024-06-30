from VolumeHandControl import HandVolumeControl

if __name__ == "__main__":
    handVolumeControl = HandVolumeControl()
    try:
        handVolumeControl.run()
    finally:
        handVolumeControl.release()
