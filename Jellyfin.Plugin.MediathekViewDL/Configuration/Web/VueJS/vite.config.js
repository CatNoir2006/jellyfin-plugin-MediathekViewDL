import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

function cssInjectedByJs() {
  return {
    name: 'css-injected-by-js',
    apply: 'build',
    enforce: 'post',
    generateBundle(opts, bundle) {
      let cssCode = '';
      let jsFile = null;
      for (const [key, value] of Object.entries(bundle)) {
        if (key.endsWith('.css')) {
          cssCode += value.source;
          delete bundle[key];
        } else if (key.endsWith('.js')) {
          jsFile = value;
        }
      }
      if (jsFile && cssCode) {
        const cssStr = JSON.stringify(cssCode);
        jsFile.code = `(function(){
          const style = document.createElement('style');
          style.textContent = ${cssStr};
          document.head.appendChild(style);
        })();\n` + jsFile.code;
      }
    }
  }
}

export default defineConfig(({ command }) => {
  if (command === 'build') {
    return {
      define: {
        'process.env.NODE_ENV': JSON.stringify('production')
      },
      plugins: [vue(), cssInjectedByJs()],
      build: {
        lib: {
          entry: 'src/main.js',
          formats: ['es'],
          fileName: () => 'MediathekViewDLVueJS.js'
        },
        outDir: 'dist',
        emptyOutDir: true
      }
    }
  }

  // Development config
  return {
    plugins: [vue()],
    server: {
      port: 5173
    }
  }
})

