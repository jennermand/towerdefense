// Game scaling functionality for responsive design
window.setupGameScaling = (gameAreaElement) => {
    // Validate input
    if (!gameAreaElement) {
        console.error('setupGameScaling: gameAreaElement is null or undefined');
        return;
    }

    const gameArea = gameAreaElement;
    const originalWidth = 800;
    const originalHeight = 600;

    function updateScale() {
        try {
            // Use full window size instead of container
            const availableWidth = window.innerWidth;
            const availableHeight = window.innerHeight;

            // Ensure we have positive dimensions
            if (availableWidth <= 0 || availableHeight <= 0) {
                console.warn('setupGameScaling: Container has no available space');
                return;
            }

            // Calculate scale to fill the entire available space
            const scaleX = availableWidth / originalWidth;
            const scaleY = availableHeight / originalHeight;
            const scale = Math.min(scaleX, scaleY); // Remove the 1.0 limit - allow scaling up!

            // Apply the scale transform
            gameArea.style.transform = `scale(${scale})`;

            // Store the current scale for click coordinate conversion
            window.currentGameScale = scale;

            console.log(`Game scaled to ${(scale * 100).toFixed(1)}%`);
        } catch (error) {
            console.error('setupGameScaling: Error in updateScale:', error);
        }
    }

    // Initial scale with a small delay to ensure DOM is ready
    setTimeout(updateScale, 100);

    // Update scale on window resize
    window.addEventListener('resize', updateScale);

    // No need for ResizeObserver since we're using window resize events
};

// Helper function to convert screen coordinates to game coordinates
window.screenToGameCoords = (screenX, screenY, gameAreaElement) => {
    const rect = gameAreaElement.getBoundingClientRect();
    const scale = window.currentGameScale || 1.0;
    
    // Calculate relative position within the scaled game area
    const relativeX = (screenX - rect.left) / scale;
    const relativeY = (screenY - rect.top) / scale;
    
    return { x: relativeX, y: relativeY };
};
