import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { ConfigProvider } from 'antd'
import './index.css'
import App from './App.tsx'
import { antDesignDarkTheme } from './app/theme/ant-design-dark-theme-config'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ConfigProvider theme={antDesignDarkTheme}>
      <App />
    </ConfigProvider>
  </StrictMode>,
)
