import {AppProps} from 'next/app';
import '@/styles/global.scss';
import Layout from '@/components/Layout';
// import '@/styles/styles.scss';

function MyApp({Component, pageProps}: AppProps) {
    return (
    <Layout title="minebea force sonsor viewer">
      <Component {...pageProps} />
    </Layout>
    );
  }

export default MyApp;