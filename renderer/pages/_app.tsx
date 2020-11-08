import {AppProps} from 'next/app';
import '../styles/global.css';
import '../styles/style.css';

function MyApp({Component, pageProps}: AppProps) {
    return <Component {...pageProps} />
  }

export default MyApp;