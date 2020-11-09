const path = require("path");

module.exports = {
//   sassOptions: {
//     includePaths: [path.join(__dirname, "./renderer/styles")],
//   },
  sassOptions: {
    includePaths: [path.resolve(__dirname, './renderer/styles/')],
  },
  webpack: (config, { buildId, dev, isServer, defaultLoaders, webpack }) => {
    console.log(config);
    config.resolve.alias['@'] = path.resolve(__dirname, './renderer');

    return config;
  },
};
