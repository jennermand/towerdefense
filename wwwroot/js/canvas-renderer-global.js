// Global Canvas Renderer for Vector TD
window.CanvasRenderer = (function() {
    let canvas = null;
    let ctx = null;

    function initialize(canvasId) {
        console.log('Initializing canvas renderer with ID:', canvasId);
        console.log('Available elements:', document.querySelectorAll('canvas'));

        canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error(`Canvas with id '${canvasId}' not found`);
            console.error('Available elements with IDs:', Array.from(document.querySelectorAll('[id]')).map(el => el.id));
            throw new Error(`Canvas with id '${canvasId}' not found`);
        }
        
        ctx = canvas.getContext('2d');
        if (!ctx) {
            console.error('2D context not supported');
            throw new Error('2D context not supported');
        }
        
        // Set canvas size
        canvas.width = 800;
        canvas.height = 600;
        
        console.log('Canvas renderer initialized successfully');
    }

    function clear(r, g, b, a) {
        if (!ctx) return;
        
        ctx.fillStyle = `rgba(${Math.floor(r * 255)}, ${Math.floor(g * 255)}, ${Math.floor(b * 255)}, ${a})`;
        ctx.fillRect(0, 0, canvas.width, canvas.height);
    }

    function beginFrame() {
        if (!ctx) return;
        ctx.save();
    }

    function drawLine(x1, y1, x2, y2, r, g, b, a, thickness) {
        if (!ctx) return;
        
        ctx.strokeStyle = `rgba(${Math.floor(r * 255)}, ${Math.floor(g * 255)}, ${Math.floor(b * 255)}, ${a})`;
        ctx.lineWidth = thickness;
        ctx.beginPath();
        ctx.moveTo(x1, y1);
        ctx.lineTo(x2, y2);
        ctx.stroke();
    }

    function drawRectangle(x, y, width, height, r, g, b, a) {
        if (!ctx) return;
        
        ctx.fillStyle = `rgba(${Math.floor(r * 255)}, ${Math.floor(g * 255)}, ${Math.floor(b * 255)}, ${a})`;
        ctx.fillRect(x, y, width, height);
    }

    function drawCircle(x, y, radius, r, g, b, a) {
        if (!ctx) return;
        
        ctx.fillStyle = `rgba(${Math.floor(r * 255)}, ${Math.floor(g * 255)}, ${Math.floor(b * 255)}, ${a})`;
        ctx.beginPath();
        ctx.arc(x, y, radius, 0, 2 * Math.PI);
        ctx.fill();
    }

    function endFrame() {
        if (!ctx) return;
        ctx.restore();
    }

    function renderFrame(commands) {
        if (!ctx || !commands) {
            console.log('No context or commands available');
            return;
        }
        
        console.log('Rendering frame with', commands.length, 'commands');
        ctx.save();
        
        for (const command of commands) {
            const commandType = command.Type || command.type;
            const params = command.Parameters || command.parameters;
            
            switch (commandType) {
                case 'clear':
                    clear(...params);
                    break;
                case 'drawLine':
                    drawLine(...params);
                    break;
                case 'drawRectangle':
                    drawRectangle(...params);
                    break;
                case 'drawCircle':
                    drawCircle(...params);
                    break;
                default:
                    console.warn('Unknown command type:', commandType);
            }
        }
        
        ctx.restore();
    }

    // Return public API
    return {
        initialize: initialize,
        clear: clear,
        beginFrame: beginFrame,
        drawLine: drawLine,
        drawRectangle: drawRectangle,
        drawCircle: drawCircle,
        endFrame: endFrame,
        renderFrame: renderFrame
    };
})();
