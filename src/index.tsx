import { createRoot } from 'react-dom/client';
import './import-library.ts';

import App from './App';
import { ConfigProvider } from '@contexts/config';
import reportWebVitals from './reportWebVitals';

const container = document.getElementById('root');
const root = createRoot(container!);

root.render(
  <ConfigProvider>
    <App />
  </ConfigProvider>
);

reportWebVitals();
