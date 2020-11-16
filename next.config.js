const path = require("path");
const fs = require('fs');

const withTM = require('next-transpile-modules')([
  'three'
]);


const alias = {
  // トラックボール
  'three/examples/jsm/controls/TrackballControls': path.join(__dirname, 'node_modules/three/examples/jsm/controls/TrackballControls.js'),
  // 物体ドラッグ
  'three/examples/jsm/controls/DragControls': path.join(__dirname, 'node_modules/three/examples/jsm/controls/DragControls.js'),
  //// カメラ制御
  'three/examples/jsm/controls/OrbitControls': path.join(__dirname, 'node_modules/three/examples/jsm/controls/OrbitControls.js'),
  //// gltf 読み込み
  'three/examples/jsm/loaders/GLTFLoader': path.join(__dirname, 'node_modules/three/examples/jsm/loaders/GLTFLoader.js'),

  '@': path.join(__dirname,  path.resolve(__dirname, './renderer')),
}
module.exports = withTM({
//   sassOptions: {
//     includePaths: [path.join(__dirname, "./renderer/styles")],
//   },
  sassOptions: {
    includePaths: [path.resolve(__dirname, './renderer/styles/')],
  },
  webpack: (config, { buildId, dev, isServer, defaultLoaders, webpack }) => {
    // console.log(config);
    // config.resolve.alias['@'] = path.resolve(__dirname, './renderer');
    config.resolve.alias = Object.assign(config.resolve.alias, alias);

    // config.module.rules.push({
    //   test: "\.jsx?$",
		// 		include: [
		// 			path.resolve(__dirname, "./node_modules/three"),
		// 		],
		// 		use: {
		// 			loader: "next-babel-loader",
		// 			options: {
		// 				isServer: false,
		// 				distDir: "C:\\Users\\1247065\\Documents\\React\\react-three-app\\renderer\\.next",
		// 				pagesDir: "C:\\Users\\1247065\\Documents\\React\\react-three-app\\renderer\\pages",
		// 				cwd: "C:\\Users\\1247065\\Documents\\React\\react-three-app\\renderer",
		// 				cache: true,
		// 				babelPresetPlugins: [],
		// 				hasModern: false,
		// 				development: false,
		// 				hasReactRefresh: false,
		// 				hasJsxRuntime: false
		// 			}
    //     }
    //   });

    // config.plugins.

    const filename = isServer? "server.json": "client.json";

    fs.writeFileSync(filename, JSON.stringify(config, null, "\t"), {encoding: "utf-8"});

     return config;
  },
});
